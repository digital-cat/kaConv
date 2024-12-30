namespace kaConvCui
{
    internal static class Program
    {
        private static bool silent = false;

        /// <summary>
        /// 過去ログ取得
        /// </summary>
        /// <param name="args">
        /// [0]:スレURL
        /// [1]:ログフォルダパス
        /// [2]:-s メッセージ非表示オプション（省略可）
        /// </param>
        /// <returns>
        /// 0:正常終了
        /// 1:キャンセル
        /// 9:起動パラメータ不正
        /// 2桁:処理エラー
        /// 3桁:HTTPステータスコード
        /// </returns>
        [STAThread]
        static int Main(string[] args)
        {
            string url = string.Empty;
            string dir = string.Empty;
            int prm = 0;

            foreach (string arg in args)
            {
                if (string.Compare(arg, "-s", true) == 0)
                {
                    silent = true;
                }
                else
                {
                    switch (prm)
                    {
                        case 0:
                            url = arg;
                            prm++;
                            break;
                        case 1:
                            dir = arg;
                            prm++;
                            break;
                    }
                }
            }

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(dir))
            {
                OutputMessage("使用方法：kaConvCui.exe スレURL ログフォルダー [-s]");
                OutputMessage("　-s：メッセージ非表示");
                return 9;   // 起動パラメータ不正
            }

            int result;

            try
            {
                using (var core = new kaConvCore.kaConvCore())
                {
                    result = core.GetLog(args[0], args[1]);

                    if (result == kaConvCore.Status.OK)
                    {
                        OutputMessage("過去スレの取得に成功しました。");
                    }
                    else
                    {
                        OutputMessage(core.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                result = 99;
                OutputMessage(ex.Message);
            }

            return result;
        }

        /// <summary>
        /// メッセージ出力
        /// </summary>
        /// <param name="msg"></param>
        private static void OutputMessage(string msg)
        {
            if (!silent)
            { 
                Console.WriteLine(msg);
            }
        }

    }
}

