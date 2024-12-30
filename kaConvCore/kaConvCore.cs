using System.Text;
using System.Net;

namespace kaConvCore
{
    /// <summary>
    /// 処理結果コード
    /// </summary>
    public class Status
    {
        /// <summary>正常終了</summary>
        public static readonly int OK = 0;
        /// <summary>キャンセル</summary>
        public static readonly int CANCEL = 1;
        /// <summary>処理エラー</summary>
        public static readonly int ERROR = 10;
        /// <summary>ダウンロード処理エラー</summary>
        public static readonly int ERRORDL = 11;
    }

    /// <summary>
    /// 過去ログ取得クラス
    /// </summary>
    public class kaConvCore : IDisposable
    {
        /// <summary>HTTPクライアント</summary>
        private static readonly HttpClient httpClient = new HttpClient();

        /// <summary>停止要求トークン</summary>
        private CancellationTokenSource tokensrc = new CancellationTokenSource();

        /// <summary>エラーメッセージ</summary>
        public string Message { get; private set; } = string.Empty;

        /// <summary>Shift-JISエンコーディング</summary>
        private Encoding SJIS;

        /// <summary>
        /// HTTPクライアントスレッド処理結果クラス
        /// </summary>
        public class TaskResult : IDisposable
        {
            /// <summary>HTTPステータスコード</summary>
            public HttpStatusCode? StatusCode { set; get; } = null;

            /// <summary>処理結果</summary>
            public bool Status => (StatusCode != null && StatusCode == HttpStatusCode.OK);

            /// <summary>取得コンテンツ</summary>
            private MemoryStream? _stream = null;

            /// <summary>取得コンテンツ</summary>
            public MemoryStream Stream
            {
                get
                {
                    if (_stream == null)
                    {
                        _stream = new MemoryStream();
                    }
                    return _stream;
                }
            }

            /// <summary>
            /// 後始末
            /// </summary>
            public void Dispose()
            {
                if (_stream != null)
                {
                    _stream.Dispose();
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public kaConvCore()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            SJIS = Encoding.GetEncoding(932);
        }

        /// <summary>
        /// 後始末
        /// </summary>
        public void Dispose()
        {
            ((IDisposable)tokensrc).Dispose();
        }

        /// <summary>
        /// 過去ログ取得
        /// </summary>
        /// <param name="url">スレURL</param>
        /// <param name="logDir">ログ保存フォルダ</param>
        /// <returns>0:正常終了／1:キャンセル／2桁:処理エラー／3桁:HTTPステータスコード</returns>
        public int GetLog(string url, string logDir)
        {
            var token = tokensrc.Token;
            int exitcode = Status.OK;

            try
            {
                Message = string.Empty;


                string kakoUrl = string.Empty;
                string boardDir = string.Empty;
                string fileName = string.Empty;

                ParseUrl(url, logDir, ref kakoUrl, ref boardDir, ref fileName);

                using (var res = Task.Run(() => Download(kakoUrl)))
                {
                    res.Wait();

                    if (res.Result.Status == false)
                    {
                        if (res.Result.StatusCode != null)
                        {
                            exitcode = (int)res.Result.StatusCode;
                            throw new Exception($"過去スレHTMLのダウンロードに失敗しました。[{exitcode}][{res.Result.StatusCode}]");
                        }
                        else
                        {
                            exitcode = Status.ERRORDL;
                            throw new Exception("過去スレHTMLのダウンロードに失敗しました。");
                        }
                    }

                    if (Directory.Exists(boardDir) == false)
                    {
                        Directory.CreateDirectory(boardDir);
                    }
                    string path = Path.Combine(boardDir, fileName);

                    OutputDat(res.Result.Stream, path);
                }
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested)
                {
                    Message = "処理がキャンセルされました。";
                    exitcode = Status.CANCEL;
                }
                else
                {
                    Message = ex.Message;
                    if (exitcode == Status.OK)
                    {
                        exitcode = Status.ERROR;
                    }
                }
            }

            return exitcode;
        }

        /// <summary>
        /// URL解析
        /// </summary>
        /// <param name="url">スレURL</param>
        /// <param name="logDir">ログ保存フォルダ（インストール先\Log\2ch を想定）</param>
        /// <param name="kakoUrl">過去ログURL</param>
        /// <param name="boardDir">板のログ保存フォルダ</param>
        /// <param name="fileName">スレのdatファイル名</param>
        /// <exception cref="Exception">URL不正</exception>
        private void ParseUrl(string url, string logDir, ref string kakoUrl, ref string boardDir, ref string fileName)
        {
            string thrUrl = url;
            int idx = thrUrl.IndexOf("://");
            if (idx >= 0)
            {
                thrUrl = "https" + thrUrl.Substring(idx);
            }
            else if (thrUrl.StartsWith("//"))
            {
                thrUrl = "https:" + thrUrl;
            }
            else if (thrUrl[0] == '/')
            {
                thrUrl = "https:/" + thrUrl;
            }
            else
            {
                throw new Exception("スレッドのURLを指定してください。");
            }

            Uri uri = new Uri(thrUrl);

            if (uri.Host.EndsWith(".5ch.net") == false && uri.Host.EndsWith(".2ch.net") == false)
            {
                throw new Exception("５ちゃんねるのスレッドURLを指定してください。");
            }

            const string cgi = "/test/read.cgi/";
            idx = uri.AbsolutePath.IndexOf(cgi);
            if (idx < 0)
            {
                throw new Exception("スレッドのURLを指定してください。");
            }
            idx += cgi.Length;
            int idx2 = uri.AbsolutePath.IndexOf("/", idx);
            if (idx2 < 0)
            {
                throw new Exception("スレッドのURLを指定してください。");
            }

            string board = uri.AbsolutePath.Substring(idx, idx2 - idx);
            string thread = uri.AbsolutePath.Substring(idx2 + 1);

            if (string.IsNullOrEmpty(board) || string.IsNullOrEmpty(thread))
            {
                throw new Exception("スレッドのURLを指定してください。");
            }

            idx = thread.IndexOf('/');
            if (idx == 0)
            {
                throw new Exception("スレッドのURLを指定してください。");
            }
            if (idx > 0)
            {
                thread = thread.Substring(0, idx);
            }

            kakoUrl = $"https://kako.5ch.net/test/read.cgi/{board}/{thread}/";
            boardDir = Path.Combine(logDir, board);
            fileName = thread + ".dat";
        }

        /// <summary>
        /// 過去スレHTMLダウンロード
        /// </summary>
        /// <param name="url">過去スレURL</param>
        /// <returns>処理結果</returns>
        private async Task<TaskResult> Download(string url)
        {
            TaskResult taskResul = new TaskResult();

            using (var response = await httpClient.GetAsync(url, tokensrc.Token))
            {
                taskResul.StatusCode = response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    using (var resStream = await response.Content.ReadAsStreamAsync())
                    {
                        resStream.CopyTo(taskResul.Stream);
                        taskResul.Stream.Position = 0;
                    }
                }
            }

            return taskResul;
        }

        /// <summary>
        /// レスdatクラス
        /// </summary>
        public class ResData
        {
            public string Name = string.Empty;
            public string Mail = string.Empty;
            public string Date = string.Empty;
            public string ID = string.Empty;
            public string Be = string.Empty;
            public string Msg = string.Empty;

            /// <summary>
            /// レスdat作成
            /// </summary>
            /// <param name="writer"></param>
            /// <param name="title"></param>
            public void Make(StringWriter writer, string title = "")
            {
                writer.Write(Name);
                writer.Write("<>");
                writer.Write(Mail);
                writer.Write("<>");
                writer.Write(Date);
                writer.Write(" ");
                writer.Write(ID);
                writer.Write(Be);
                writer.Write("<>");
                writer.Write(Msg);
                writer.Write("<>");
                writer.Write(title);
                writer.Write("\r\n");
            }
        }

        /// <summary>
        /// datファイル出力
        /// </summary>
        /// <param name="stream">HTML</param>
        /// <param name="path">出力ファイルパス</param>
        private void OutputDat(MemoryStream stream, string path)
        {
            string html = GetStringFromSjis(stream);

            using (StringWriter writer = new StringWriter())
            {
                string title = string.Empty;
                int start = 0;
                Extract(ref html, "<h1 id=\"threadtitle\">", "</h1>", ref title, ref start);
                title = title.Trim();

                // レス番号でループ
                for (int no = 1; no < 10000; no++)
                {
                    // 1レス分のHTMLを抽出
                    string res = string.Empty;
                    if (Extract(ref html, $"<div id=\"{no}\"", "</div>", ref res, ref start) == false)
                    {
                        break;
                    }

                    ResData data = new ResData();

                    string temp = string.Empty;
                    int idx = 0;
                    Extract(ref res, "<span class=\"postusername\">", "</span>", ref temp, ref idx);
                    idx = 0;
                    Extract(ref temp, "<a rel=\"nofollow\" href=\"mailto:", "\"", ref data.Mail, ref idx);
                    data.Name = TrimTag(TrimTag(temp, "a"), "b").Trim();
                    idx = 0;
                    Extract(ref res, "<span class=\"date\">", "</span>", ref data.Date, ref idx);
                    idx = 0;
                    Extract(ref res, "<span class=\"uid\">", "</span>", ref data.ID, ref idx);
                    idx = 0;
                    temp = string.Empty;
                    if (Extract(ref res, "<span class=\"be r2BP\">", "</span>", ref temp, ref idx))
                    {
                        string be1 = string.Empty;
                        string be2 = string.Empty;
                        idx = 0;
                        if (Extract(ref temp, "ch.net/user/", "\"", ref be1, ref idx) && string.IsNullOrEmpty(be1) == false &&
                            Extract(ref temp, ">?", "</a>", ref be2, ref idx) && string.IsNullOrEmpty(be2) == false)
                        {
                            data.Be = $" BE:{be1}-{be2}";
                        }
                    }
                    idx = 0;
                    temp = string.Empty;
                    if (Extract(ref res, "<section class=\"post-content\">", "</section>", ref temp, ref idx))
                    {
                        data.Msg = ConvSssp(TrimLink(temp));
                    }

                    data.Make(writer, (no == 1) ? title : string.Empty);
                }

                File.WriteAllText(path, writer.ToString(), SJIS);
            }
        }

        /// <summary>
        /// Shift-JISデータからUTF-16文字列へ変換
        /// </summary>
        /// <param name="stream">Shift-JISデータ</param>
        /// <returns>UTF-16文字列</returns>
        private string GetStringFromSjis(MemoryStream stream)
        {
            stream.Position = 0;
            return SJIS.GetString(stream.ToArray());
        }

        /// <summary>
        /// キーワード指定文字列抽出
        /// </summary>
        /// <param name="src">元文字列</param>
        /// <param name="kwSt">開始キーワード</param>
        /// <param name="kwEd">終了キーワード</param>
        /// <param name="data">抽出文字列</param>
        /// <param name="start">キーワード検索開始位置</param>
        /// <returns>成功／失敗</returns>
        private bool Extract(ref string src, string kwSt, string kwEd, ref string data, ref int start)
        {
            int idx1 = src.IndexOf(kwSt, start);
            if (idx1 < 0)
            {
                return false;
            }
            idx1 += kwSt.Length;
            int idx2 = src.IndexOf(kwEd, idx1);
            if (idx2 < 0)
            {
                return false;
            }

            data = src.Substring(idx1, idx2 - idx1);

            start = idx2 + kwEd.Length;

            return true;
        }

        /// <summary>
        /// 指定HTMLタグ削除
        /// </summary>
        /// <param name="src">HTML文字列</param>
        /// <param name="tag">タグ名（"a"や"img"など）</param>
        /// <returns>処理結果文字列</returns>
        private string TrimTag(string src, string tag)
        {
            string tmp = src;

            while (true)
            {
                int idx1 = tmp.IndexOf($"<{tag}");
                if (idx1 < 0)
                {
                    break;
                }
                int idx2 = tmp.IndexOf(">", idx1);
                if (idx2 < 0)
                {
                    break;
                }

                if (idx1 == 0)
                {
                    tmp = tmp.Substring(idx2 + 1);
                }
                else
                {
                    tmp = tmp.Substring(0, idx1) + tmp.Substring(idx2 + 1);
                }
            }

            return tmp.Replace($"</{tag}>", "");
        }

        /// <summary>
        /// リンクタグ削除（アンカーを除外）
        /// </summary>
        /// <param name="src">HTML文字列（レス本文を想定）</param>
        /// <returns>処理結果文字列</returns>
        private string TrimLink(string src)
        {
            const string tagE = "</a>";
            string tmp = src;
            int start = 0;

            while (true)
            {
                int idx1 = tmp.IndexOf($"<a ", start);
                if (idx1 < 0)
                {
                    break;
                }
                int idx2 = tmp.IndexOf(">", idx1);
                if (idx2 < 0)
                {
                    break;
                }
                int idx3 = tmp.IndexOf("href=\"../test/read.cgi/", idx1);
                if (idx3 > 0 && idx3 < idx2)
                {
                    start = idx2;
                    continue;
                }
                int idx4 = tmp.IndexOf(tagE, idx2);
                if (idx4 > 0)
                {
                    tmp = tmp.Substring(0, idx4) + tmp.Substring(idx4 + tagE.Length);
                }

                if (idx1 == 0)
                {
                    tmp = tmp.Substring(idx2 + 1);
                }
                else
                {
                    tmp = tmp.Substring(0, idx1) + tmp.Substring(idx2 + 1);
                }
            }

            return tmp;
        }

        /// <summary>
        /// sssp画像タグ変換
        /// </summary>
        /// <param name="src">HTML文字列</param>
        /// <returns>処理結果文字列</returns>
        private string ConvSssp(string src)
        {
            string tmp = src;
            const string tagS2 = "<img src=\"//img.2ch.net/";
            const string tagS5 = "<img src=\"//img.5ch.net/";
            const string tagE = "\">";

            while (true)
            {
                int tagLen;
                int idx1 = tmp.IndexOf(tagS2);
                if (idx1 >= 0)
                {
                    tagLen = tagS2.Length; 
                }
                else
                {
                    idx1 = tmp.IndexOf(tagS5);
                    if (idx1 < 0)
                    {
                        break;
                    }
                    tagLen = tagS5.Length;
                }
                int idx2 = tmp.IndexOf(tagE, idx1);
                if (idx2 < 0)
                {
                    break;
                }

                int idx = idx1 + tagLen;
                string url = "sssp://img.5ch.net/" + tmp.Substring(idx, idx2 - idx);

                idx2 += tagE.Length;

                if (idx1 == 0)
                {
                    tmp = url + tmp.Substring(idx2);
                }
                else
                {
                    tmp = tmp.Substring(0, idx1) + url + tmp.Substring(idx2);
                }
            }

            return tmp;
        }

    }
}
