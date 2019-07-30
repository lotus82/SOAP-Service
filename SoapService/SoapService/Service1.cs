using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Text;
using System.Xml;

namespace SoapService
{
    public partial class Service1 : ServiceBase
    {
        Encoding code = new UTF8Encoding();
        string _soapEnvelope = "";
        string ft = "";
        string host = "";
        string ext = "";
        string method = "";
        string username = "";
        string password = "";
        string system_id = "";
        string status_string = "";
        string ls_string = "";

        public Service1()
        {
            InitializeComponent();
        }

        private string GetResponseSoap(string _url, string _method, string _soapEnvelope)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            _url = _url.Trim('/').Trim('\\'); 
            WebRequest _request = HttpWebRequest.Create(_url);
            _request.Method = "POST";
            _request.ContentType = "text/xml; charset=utf-8";
            _request.ContentLength = _soapEnvelope.Length;
            String username = "*****";
            String password = "*****";
            String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
            _request.Headers.Add("Authorization", "Basic " + encoded);
            _request.Credentials = new NetworkCredential("*****", "*****");
            StreamWriter _streamWriter = new StreamWriter(_request.GetRequestStream());
            _streamWriter.Write(_soapEnvelope);
            _streamWriter.Close();
            WebResponse _response = _request.GetResponse();
            StreamReader _streamReader = new StreamReader(_response.GetResponseStream(), System.Text.Encoding.UTF8);
            string _result = _streamReader.ReadToEnd();
            return _result;
        }

        private void LS_Request()
        {
            ls_string = "";
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(@"" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\request_xmlservice_" + ft + @".xml");
            XmlElement xRoot = xDoc.DocumentElement;
            foreach (XmlNode childnode in xRoot)
            {
                if (childnode.Name == "soapenv:Body")
                {
                    foreach (XmlNode childnode2 in childnode)
                    {
                        if (childnode2.Name == "ext:setBillingCheckRequest")
                        {
                            foreach (XmlNode childnode3 in childnode2)
                            {
                                if (childnode3.Name == "ext:receipt")
                                {
                                    foreach (XmlNode childnode4 in childnode3)
                                    {
                                        if (childnode4.Name == "ext:client")
                                        {
                                            foreach (XmlNode childnode5 in childnode4)
                                            {
                                                if (childnode5.Name == "ext:ls_number")
                                                {
                                                    ls_string = ls_string + "Лицевой счет: " + childnode5.InnerText + "; ";
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Status_Response()
        {
            status_string = "";
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(@"" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) +@"\response_xmlservice_" + ft + @".xml");
            XmlElement xRoot = xDoc.DocumentElement;
            foreach (XmlNode childnode in xRoot)
            {
                if (childnode.Name == "SOAP-ENV:Body")
                {
                    foreach (XmlNode childnode2 in childnode)
                    {
                        if (childnode2.Name == "ns2:setBillingCheckResponse")
                        {
                            foreach (XmlNode childnode3 in childnode2)
                            {
                                if (childnode3.Name == "ns2:status")
                                {
                                    status_string = status_string + "Статус: " + childnode3.InnerText + "; ";
                                }
                                if (childnode3.Name == "ns2:message")
                                {
                                    if (childnode3.InnerText.Length > 0)
                                    {
                                        status_string = status_string + "Сообщение: " + childnode3.InnerText;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private string CreateRequestSoap()//Tuple<int, string>
        {
            INIManager manager = new INIManager(@"" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\conf.ini");// + @"\conf.ini");
            host = manager.GetPrivateString("Connection", "host");
            ext = manager.GetPrivateString("Connection", "ext");
            method = manager.GetPrivateString("Connection", "method");
            username = manager.GetPrivateString("Auth", "username");
            password = manager.GetPrivateString("Auth", "password");
            system_id = manager.GetPrivateString("Auth", "system_id");
         
            _soapEnvelope =
            @"<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:ext='" + ext + @"'>
                <soapenv:Header/>
                <soapenv:Body>
                    <ext:setBillingCheckRequest>
                        <ext:system_id>" + system_id + @"</ext:system_id>
                        <ext:timestamp>08.05.2019 22:10</ext:timestamp>
                        <ext:external_id>123456309978</ext:external_id>
                        <ext:receipt>
                            <ext:client>
                                <ext:ls_number>ls1</ext:ls_number>
                            </ext:client>
                            <ext:items>
                                <ext:item>
                                    <ext:name>Tovar1</ext:name>
                                    <ext:price>22.10</ext:price>
                                    <ext:quantity>2</ext:quantity>
                                    <ext:sum>44.20</ext:sum>
                                    <ext:measurement_unit>en</ext:measurement_unit>
                                    <ext:payment_method>full_payment</ext:payment_method>
                                    <ext:payment_object>service</ext:payment_object>
                                    <ext:vat>
                                        <ext:type>vat20</ext:type>
                                        <ext:sum>4.10</ext:sum>
                                    </ext:vat>
                                    <ext:supplier_info>
                                        <ext:orgn>ogrn1</ext:orgn>
                                    </ext:supplier_info>
                                </ext:item>
                                <ext:item>
                                    <ext:name>Tovar2</ext:name>
                                    <ext:price>10</ext:price>
                                    <ext:quantity>1</ext:quantity>
                                    <ext:sum>20</ext:sum>
                                    <ext:measurement_unit>en</ext:measurement_unit>
                                    <ext:payment_method>full_payment</ext:payment_method>
                                    <ext:payment_object>service</ext:payment_object>
                                    <ext:vat>
                                        <ext:type>vat20</ext:type>
                                        <ext:sum>4</ext:sum>
                                    </ext:vat>
                                </ext:item>
                            </ext:items>
                            <ext:payments>
                                <ext:payment>
                                    <ext:type>1</ext:type>
                                    <ext:sum>64.20</ext:sum>
                                </ext:payment>
                            </ext:payments>
                            <ext:vats>
                                <ext:vat>
                                    <ext:type>vat20</ext:type>
                                    <ext:sum>8.10</ext:sum>
                                </ext:vat>
                            </ext:vats>
                            <ext:total>64.20</ext:total>
                        </ext:receipt>
                    </ext:setBillingCheckRequest>
                    <ext:setBillingCheckRequest>
                        <ext:system_id>" + system_id + @"</ext:system_id>
                        <ext:timestamp>08.05.2019 22:10</ext:timestamp>
                        <ext:external_id>123456309978</ext:external_id>
                        <ext:receipt>
                            <ext:client>
                                <ext:ls_number>ls2</ext:ls_number>
                            </ext:client>
                            <ext:items>
                                <ext:item>
                                    <ext:name>Tovar1</ext:name>
                                    <ext:price>22.10</ext:price>
                                    <ext:quantity>2</ext:quantity>
                                    <ext:sum>44.20</ext:sum>
                                    <ext:measurement_unit>en</ext:measurement_unit>
                                    <ext:payment_method>full_payment</ext:payment_method>
                                    <ext:payment_object>service</ext:payment_object>
                                    <ext:vat>
                                        <ext:type>vat20</ext:type>
                                        <ext:sum>4.10</ext:sum>
                                    </ext:vat>
                                    <ext:supplier_info>
                                        <ext:orgn>ogrn1</ext:orgn>
                                    </ext:supplier_info>
                                </ext:item>
                                <ext:item>
                                    <ext:name>Tovar2</ext:name>
                                    <ext:price>10</ext:price>
                                    <ext:quantity>1</ext:quantity>
                                    <ext:sum>20</ext:sum>
                                    <ext:measurement_unit>en</ext:measurement_unit>
                                    <ext:payment_method>full_payment</ext:payment_method>
                                    <ext:payment_object>service</ext:payment_object>
                                    <ext:vat>
                                        <ext:type>vat20</ext:type>
                                        <ext:sum>4</ext:sum>
                                    </ext:vat>
                                </ext:item>
                            </ext:items>
                            <ext:payments>
                                <ext:payment>
                                    <ext:type>1</ext:type>
                                    <ext:sum>64.20</ext:sum>
                                </ext:payment>
                            </ext:payments>
                            <ext:vats>
                                <ext:vat>
                                    <ext:type>vat20</ext:type>
                                    <ext:sum>8.10</ext:sum>
                                </ext:vat>
                            </ext:vats>
                            <ext:total>64.20</ext:total>
                        </ext:receipt>
                    </ext:setBillingCheckRequest>
                </soapenv:Body>
            </soapenv:Envelope>";
            ServicePointManager.ServerCertificateValidationCallback = ValidateRemoteCertificate;
            string _response = GetResponseSoap(@host, @method, _soapEnvelope); // получаем ответ SOAP сервиса в виде XML
            File.WriteAllText(@"" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\request_xmlservice_" + ft + @".xml", _soapEnvelope); // записываем запрос в файл
            File.WriteAllText(@"" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\response_xmlservice_" + ft + @".xml", _response); // записываем ответ в файл
            LS_Request(); // извлекаем номер Л/С из запроса
            Status_Response(); // извлекаем статус и сообщение (если error) из ответа
            File.AppendAllText(@"" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\SoapService.log", ls_string + status_string + DateTime.Now + "\r\n", code);
            return _response;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                timer2.Start();
                File.AppendAllText(@"" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\SoapService.log", "Запуск службы " + DateTime.Now + "\r\n", code);
                CreateRequestSoap();
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\SoapService.log", "Ошибка запуска службы: " + ex.Message + ". " + DateTime.Now + "\r\n", code); 
            }
        }

        protected override void OnStop()
        {
            try
            { 
                timer2.Stop();
                File.AppendAllText(@"" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\SoapService.log", "Остановка службы " + DateTime.Now + "\r\n", code);
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\SoapService.log", "Ошибка остановки службы: " + ex.Message + ". " + DateTime.Now + "\r\n", code); 
            }
        }

        public static bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private void timer2_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer2.Stop();
            try
            {
                if (CreateRequestSoap().Length < 1)
                    {
                        timer2.Start();
                    }
            }
            catch (Exception ex)
            {
                File.AppendAllText(@"" + System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + @"\SoapService.log", "Ошибка получения данных: " + ex.Message + ". " + DateTime.Now + "\r\n", code);
                timer2.Start();
            }
        }
    }

    public class INIManager
    {
        //Конструктор, принимающий путь к INI-файлу
        public INIManager(string aPath)
        {
            path = aPath;
        }

        //Конструктор без аргументов (путь к INI-файлу нужно будет задать отдельно)
        public INIManager() : this("") { }

        //Возвращает значение из INI-файла (по указанным секции и ключу) 
        public string GetPrivateString(string aSection, string aKey)
        {
            //Для получения значения
            StringBuilder buffer = new StringBuilder(SIZE);

            //Получить значение в buffer
            GetPrivateString(aSection, aKey, null, buffer, SIZE, path);

            //Вернуть полученное значение
            return buffer.ToString();
        }

        //Пишет значение в INI-файл (по указанным секции и ключу) 
        public void WritePrivateString(string aSection, string aKey, string aValue)
        {
            //Записать значение в INI-файл
            WritePrivateString(aSection, aKey, aValue, path);
        }

        //Возвращает или устанавливает путь к INI файлу
        public string Path { get { return path; } set { path = value; } }

        //Поля класса
        private const int SIZE = 1024; //Максимальный размер (для чтения значения из файла)
        private string path = null; //Для хранения пути к INI-файлу

        //Импорт функции GetPrivateProfileString (для чтения значений) из библиотеки kernel32.dll
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
        private static extern int GetPrivateString(string section, string key, string def, StringBuilder buffer, int size, string path);

        //Импорт функции WritePrivateProfileString (для записи значений) из библиотеки kernel32.dll
        [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
        private static extern int WritePrivateString(string section, string key, string str, string path);
    }

   
}
