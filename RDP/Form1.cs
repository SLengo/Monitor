using System;
using System.Timers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Diagnostics;
using System.IO;
using System.Collections;
using RDotNet;

namespace RDP
{
    public partial class Form1 : Form
    {
         ArrayList xForPlot = new ArrayList();
        
        int howManyDaysBetween = 0;
        DateTime firstDate;
        DateTime secondDate;
        bool isFirstOpen = true;
        string selPath = "";
        bool whichTextbox = true;
        string maxdate = "";
        string mindate = "";
        int startAnalisisSec = 0;
        int endAnalisisSec = 0;
        int startAnalisisIdx = 0;
        int endAnalisisIdx = 0;
        double countOfDays = 0;

        StreamWriter mSW = new StreamWriter("output.txt");


        ManagementScope myScope; // создаём обект типа managementscope для созлания окружения подключения
        ConnectionOptions options; // объект для создания параметров для подключения, а именно логин и пароль
        List<ObjectQuery> query = new List<ObjectQuery>(6);
        List<ManagementObjectSearcher> searcher = new List<ManagementObjectSearcher>(6);
        List<ManagementObjectCollection> queryCollection = new List<ManagementObjectCollection>(6);
        
        

       //очереди соллекций куда будут возвтращать объекты поиска найденную информацию
        int NumOfCores; // переменная для подсчёта количества логических процессоров на сервере

        //ArrayList arrayListForScopes = new ArrayList();
        //ArrayList arrayListForOptions = new ArrayList();

        List<ManagementScope> listForScopes = new List<ManagementScope>(); //стуктура list для хранения всех окружений для подключения
        //List<ConnectionOptions> listForOptions = new List<ConnectionOptions>();

        String stringForLog = ""; // строка для записи лога
        String stringForLogProcesses = ""; // строка для записи всех процессов из listbox1, далее stringForLogProcesses используется для записи лога 
       
        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
           
        }

       

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Информация об использовании RAM и загрузки ядер процессора требует обновления, поэтому labels с соответствующей информацией
            // обновляются в методе timer1_Tick с соответствующим интервалом, задающимся первой строкой в cfg.txt
            query[5] = new ObjectQuery("select * from Win32_PerfFormattedData_PerfDisk_LogicalDisk");
            query[2] = new ObjectQuery("select * from Win32_PerfRawData_PerfOS_Memory"); // Данной строкой объявляется объект типа ObjectQuery 
            // для записи объектов класса Win32_PerfRawData_PerfOS_Memory
            searcher[2] = new ManagementObjectSearcher(myScope, query[2]); // новый объект типа ManagementObjectSearcher для поиска объектов класса Win32_PerfRawData_PerfOS_Memory
            queryCollection[2] = searcher[2].Get(); // метод Get() возвратил коллекцию объектов класса Win32_PerfRawData_PerfOS_Memory

            searcher[5] = new ManagementObjectSearcher(myScope, query[5]);

            query[0] = new ObjectQuery
                ("select * from Win32_PerfRawData_PerfOS_Processor");
            searcher[0] = new ManagementObjectSearcher(myScope, query[0]);
            queryCollection[0] = searcher[0].Get(); // аналогично, только другой класс

            query[4] = new ObjectQuery("select * from Win32_PerfFormattedData_Tcpip_NetworkInterface");
            searcher[4] = new ManagementObjectSearcher(myScope, 
                query[4]);
            queryCollection[4] = searcher[4].Get(); // аналогично, только другой класс

            queryCollection[5] = searcher[5].Get();

            label4.Text = "";
            label6.Text = "";
            int i = 0;
            // по ключу в каждой queryCollection находим соответсвующую информацию 
            // и пишем в соответствующий label
            foreach (ManagementObject m in queryCollection[2])
            {
                label6.Text = string.Format("{0}", m["AvailableMBytes"]);
            }
            foreach (ManagementObject m in queryCollection[0])
            {
                if (i < NumOfCores)
                    label4.Text += string.Format("{0} % CPU Core {1}{2}", m["DPCRate"], i, Environment.NewLine);
                else
                    label4.Text += string.Format("{0} % CPU Total{1}", m["DPCRate"], Environment.NewLine);
                i++;
            }
            foreach (ManagementObject m in queryCollection[4])
            {
                
                double currentBW = Convert.ToDouble(m["CurrentBandwidth"]);
                double totalBytesPS = Convert.ToDouble(m["BytesTotalPersec"]);
                if (totalBytesPS == 0) break;
                double networkСongestion = totalBytesPS / currentBW * 100;
                label9.Text = string.Format("{0}", networkСongestion);
            }

            foreach (ManagementObject m in queryCollection[5])
            {
                label12.Text = string.Format("{0}", m["FreeMegabytes"]);
            }

            stringForLogProcesses = "";
            foreach (object o in listBox1.Items)
            {
                stringForLogProcesses += (string)o + Environment.NewLine;
            }

            String ipForLog = listBox2.Items[listBox2.SelectedIndex].ToString();

            // Фомируется строка для лога
            stringForLog = "****Южно-Уральский государстуенный университет****" + Environment.NewLine
                + "****Задание на практику студентов группы ВМИ - 303, 2015 г.****" + Environment.NewLine
                + Environment.NewLine + "Системная информация сервера: " + ipForLog.Substring(0, ipForLog.Length - 6) + Environment.NewLine
                + "Модель процессора:" + label2.Text + Environment.NewLine
                + "Загруженность процессора:" + Environment.NewLine
                + label4.Text + Environment.NewLine
                + "Количество свободной оперативной памяти (Мб): " + label6.Text + Environment.NewLine
                + "Загруженность сети: " + label9.Text + Environment.NewLine
                + "Количество свободного места на жёстком диске, Мб: " + label12.Text + Environment.NewLine
                + "Текущие процессы: " + Environment.NewLine
                + stringForLogProcesses;
            


        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {

           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 6; ++i) {
                ManagementObjectCollection q = null;
                queryCollection.Add(q);
                ManagementObjectSearcher s = null;
                searcher.Add(s);
                ObjectQuery o = null;
                query.Add(o);
            }




                progressBar1.Visible = true;
            //progressBar1.Value = 0;
           



            // Обновление списка серверов
            timer1.Enabled = false;
            listForScopes.Clear();
            if (File.Exists("cfg.txt") == true) // Если такой файл существует, то открыть, иначе messagebox с ошибкой
            {
                int lineCnt = File.ReadLines("cfg.txt").Count();
                progressBar1.Maximum = lineCnt;
                progressBar1.Step = 1;
                listBox2.Items.Clear();
                StreamReader myReader = new StreamReader("cfg.txt"); // Создание обхекта StreamReader для чтения из cfg.txt списка серверов и интервала tick для timer_1
                String stringForSplit = null;
                stringForSplit = myReader.ReadLine(); // Читаем первую строку и изменяем timer1.Interval
                timer1.Interval = Convert.ToInt32(stringForSplit);
                while ((stringForSplit = myReader.ReadLine()) != null) // Далее, пока можно считывать строки
                {
                    String[] myMass = stringForSplit.Split(' '); // Разбиваем каждую строку по пробелам и записываем в массив по очереди: ip, username, password
                    String forScope = "\\\\" + myMass[0] + "\\root\\cimv2";
                    ConnectionOptions options1 = new ConnectionOptions();   
                    options1.Username = myMass[1];
                    options1.Password = myMass[2];
                    ManagementScope myScope1 = new ManagementScope(forScope, options1);
                    listForScopes.Add(myScope1);
                    //listBox2.Items.Add(string.Format("{0} offline", myMass[0]));

                 

                    try // Пробуем подключится и i-му серверу, исли подключение создается удачно, то в строке с сервер online иначе offline
                    {
                        ///// Вот тут пока блочу подключение, чтобы не ждать пока пропарситься cfg

                        myScope1.Connect();
                        listBox2.Items.Add(string.Format("{0} online", myMass[0]));
                    }
                    catch
                    {
                        listBox2.Items.Add(string.Format("{0} offline", myMass[0]));
                    }

                    ///// Вот тут пока блочу подключение, чтобы не ждать пока пропарситься cfg
                    progressBar1.PerformStep();


                }
                myReader.Close(); //Закрываем поток на чтение
            }
            else
            {
                MessageBox.Show("File not found!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            progressBar1.Visible = false;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            // подключение к выбранному серверу в listbox2
            if (listBox2.SelectedIndex != -1)
            {
                String strForConnectButton = listBox2.Items[listBox2.SelectedIndex].ToString();
                String[] forCheck = strForConnectButton.Split(' ');
                String[] whichServer = forCheck[0].Split('.');
                if (forCheck[1] == "offline") //Если выбранный сервер offline, подключения не произойдёт
                {
                    MessageBox.Show("Selected server is offline!\nAnalisis avalible", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    selPath = whichServer[whichServer.Length - 1];
                   
                    button4.Visible = true;
                    //richTextBox1.Visible = true;
                    label14.Visible = false;
                    label13.Visible = true;
                    label15.Visible = true;
                    textBox1.Visible = true;
                    textBox2.Visible = true;
                    //richTextBox2.Visible = true;
                    radioButton1.Visible = true;
                    radioButton2.Visible = true;
                   // menuStrip1.Visible = true;
                    //label22.Visible = true;

                    
                    label11.Visible = true;
                    button3.Visible = false;
                    label1.Visible = false;
                    label2.Visible = false;
                    label3.Visible = false;
                    label4.Visible = false;
                    label5.Visible = false;
                    label6.Visible = false;
                    label7.Visible = false;
                    label8.Visible = false;
                    label9.Visible = false;
                    label10.Visible = false;
                    listBox1.Visible = false;
                    label12.Visible = false;




                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = 100;
                    progressBar1.Value = 0;
                    progressBar1.Visible = true;
                    parserFolder();
                    
                    
                }
                else
                {
                    // Если сервер online
                    selPath = whichServer[whichServer.Length - 1];
                    tabPage1.Show();// Показать вкладку с информацией сразу после клика на кнопку Connect
                    myScope = listForScopes[listBox2.SelectedIndex]; // Подключится к выбранному серверу                    
                    try
                    {
                        myScope.Connect();

                        radioButton1.Visible = true;
                        radioButton2.Visible = true;

                        button4.Visible = true;
                        label13.Visible = true;
                        label15.Visible = true;
                        textBox1.Visible = true;
                        textBox2.Visible = true;

                        //richTextBox1.Visible = true;
                        label14.Visible = false;
                        label11.Visible = false;
                        button3.Visible = true;
                        label1.Visible = true;
                        label2.Visible = true;
                        label3.Visible = true;
                        label4.Visible = true;
                        label5.Visible = true;
                        label6.Visible = true;
                        label7.Visible = true;
                        label8.Visible = true;
                        label9.Visible = true;
                        label10.Visible = true;
                        //richTextBox2.Visible = true;

                      
                        label22.Visible = true;

                        label12.Visible = true;
                        listBox1.Visible = true; // Показать скрытые элементы


                        timer1.Enabled = true; // Запустить таймер

                        button3.Enabled = true;

                        query[1] = new ObjectQuery("select * from Win32_PerfFormattedData_PerfProc_Process"); // Аналогично, только с другим классов

                        query[3] = new ObjectQuery("select * from Win32_Processor");
                        searcher[1] = new ManagementObjectSearcher(myScope, query[1]);

                        searcher[3] = new ManagementObjectSearcher(myScope, query[3]);
                        queryCollection[1] = searcher[1].Get();

                        queryCollection[3] = searcher[3].Get();
                        //listBox1.Items.Clear();
                        foreach (ManagementObject m in queryCollection[1])
                        {
                            listBox1.Items.Add(string.Format("{0}.exe", m["Name"]));
                        }

                        foreach (ManagementObject m in queryCollection[3])
                        {
                            label2.Text = string.Format("{0}", m["Name"]);
                            NumOfCores = Convert.ToInt32(m["NumberOfLogicalProcessors"]);
                        }



                        //progressBar1.Minimum = 0;
                        //progressBar1.Maximum = 100;
                        //progressBar1.Value = 0;
                        //progressBar1.Visible = true;
                        parserFolder();

                    }
                    catch
                    {
                        MessageBox.Show("Cannot connect to server!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            //Сохранить лог
            // лог сохраняется с информацией на момент нажатия кнопки Save Log
            timer1.Enabled = false;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.Title = "Save log";
            saveFileDialog1.ShowDialog();
            if (saveFileDialog1.FileName != "")
            {
                File.WriteAllText(saveFileDialog1.FileName, stringForLog);
            }
            timer1.Enabled = true;
        }

        private void Form1_ResizeBegin(object sender, EventArgs e)
        {
            
            
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            tabControl1.Size = this.Size;

            progressBar1.Location = new Point(this.Size.Width / 2 - label11.Size.Width / 2, this.Size.Height / 2 - label11.Size.Height / 2 - label11.Size.Height);

            label11.Location = new Point(this.Size.Width / 2 - label11.Size.Width / 2, this.Size.Height / 2 - label11.Size.Height / 2 - label11.Size.Height);
            label14.Location = new Point(this.Size.Width / 2 - label14.Size.Width / 2, this.Size.Height / 2 - label14.Size.Height / 2 - label14.Size.Height);

            listBox2.Size = new Size(this.Size.Width - 40, this.Size.Height - 100);
            button1.Location = new Point(9, this.Size.Height - 95);
            button2.Location = new Point(134, this.Size.Height - 95);
            button3.Location = new Point(this.Size.Width - 150, 6);
            //richTextBox1.Size = new Size(this.Size.Width - 40, this.Size.Height - 130);
            //richTextBox2.Size = new Size(this.Size.Width - 40, this.Size.Height - richTextBox2.Size.Height / 2);


            listBox1.Size = new Size(this.Size.Width - 300, this.Size.Height - 250);
            label8.Location = new Point(241, this.Size.Height - 125);
            label9.Location = new Point(376, this.Size.Height - 125);
            label10.Location = new Point(241, this.Size.Height - 100);
            label12.Location = new Point(390, this.Size.Height - 100);                      
        }

        private void parserFolder() {
            //////////////// Парсинг папки с логами

            string[] file_list = Directory.GetFiles(@"C:\logs\" + selPath);
            bool countForEach = false;
            
            progressBar1.Visible = true;
            progressBar1.Maximum = file_list.Length;
            progressBar1.Step = 1;
            xForPlot.Clear();
            foreach (string file_to_read in file_list)
            {
               
                ArrayList fMass = new ArrayList();
                fMass.Clear();

                if (File.Exists(file_to_read) == true)
                {
                    //richTextBox1.Text = "";
                    StreamReader myReader = new StreamReader(file_to_read);
                    String myStr = null;
                    while ((myStr = myReader.ReadLine()) != null)
                    {
                        String[] splitMass = myStr.Split(' ');
                        if ((splitMass[0] == "===") && (splitMass[1] != "Started") && (splitMass[1] != "Done"))
                        {
                            fMass.Add(splitMass[5]);

                            fMass.Add(splitMass[6]);

                            fMass.Add(splitMass[7]);

                        }
                        if ((splitMass.Length > 1) && (splitMass[1] == "Duration:"))
                        {
                            int i = 0;
                            foreach (string str in splitMass)
                            {
                                if (str == "usage:")
                                    fMass.Add(splitMass[i + 1]);
                                i++;
                            }


                        }

                    }


                    if (fMass.Count > 3)
                    {
                        string[] datas = Convert.ToString(fMass[0]).Split('/');
                        if (countForEach == false) mindate = Convert.ToString(fMass[0]);
                        else maxdate = Convert.ToString(fMass[0]);
                        int whichDay = Convert.ToInt32(datas[1], 10);

                        string[] sHours = Convert.ToString(fMass[1]).Split(':');
                        int intSec = 0;
                        for (int i = 0; i < sHours.Length; i++)
                        {
                            if (i == 0)
                            {
                                if ((Convert.ToInt32(sHours[i]) == 12) && (Convert.ToString(fMass[2]) == "AM")) { }
                                else intSec += Convert.ToInt32(sHours[i], 10) * 60 * 60;
                            }
                            if (i == 1) intSec += Convert.ToInt32(sHours[i], 10) * 60;
                            if (i == 2) intSec += Convert.ToInt32(sHours[i], 10);
                        }

                        if ((Convert.ToString(fMass[2]) != "AM") && (Convert.ToInt32(sHours[0]) != 12))
                        {
                            intSec += 43200;
                        }

                        if (whichDay != 29)
                        {
                            intSec += whichDay * 86400;
                        }

                        xForPlot.Add(intSec);

                        sHours = Convert.ToString(fMass[4]).Split(':');
                        datas = Convert.ToString(fMass[3]).Split('/');
                        whichDay = Convert.ToInt32(datas[1], 10);
                        intSec = 0;
                        for (int i = 0; i < sHours.Length; i++)
                        {
                            if (i == 0)
                            {
                                if ((Convert.ToInt32(sHours[i]) == 12) && (Convert.ToString(fMass[5]) == "AM")) { }
                                else intSec += Convert.ToInt32(sHours[i], 10) * 60 * 60;
                            }
                            if (i == 1) intSec += Convert.ToInt32(sHours[i], 10) * 60;
                            if (i == 2) intSec += Convert.ToInt32(sHours[i], 10);
                        }

                        if ((Convert.ToString(fMass[5]) != "AM") && (Convert.ToInt32(sHours[0]) != 12))
                        {
                            intSec += 43200;
                        }

                        if (whichDay != 29)
                        {
                            intSec += whichDay * 86400;
                        }

                        xForPlot.Add(intSec);

                        sHours = Convert.ToString(fMass[6]).Split('%');
                        intSec = Convert.ToInt32(sHours[0], 10);

                        xForPlot.Add(intSec);
                        countForEach = true;
                    }

                }
                else
                {
                    MessageBox.Show("File not found!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

               
                progressBar1.PerformStep();
            }



            for (int i = 0; i < xForPlot.Count - 3; i += 3)
            {
                for (int j = i; j < xForPlot.Count - 3; j += 3)
                {
                    if (Convert.ToInt32(xForPlot[j]) > Convert.ToInt32(xForPlot[j + 3]))
                    {
                        object tmp = xForPlot[j];
                        xForPlot[j] = xForPlot[j + 3];
                        xForPlot[j + 3] = tmp;

                        tmp = xForPlot[j + 1];
                        xForPlot[j + 1] = xForPlot[j + 4];
                        xForPlot[j + 4] = tmp;

                        tmp = xForPlot[j + 2];
                        xForPlot[j + 2] = xForPlot[j + 5];
                        xForPlot[j + 5] = tmp;
                    }
                }
            }
            progressBar1.Visible = false;
            string[] checkMax = maxdate.Split('/');
            string tmp1 = checkMax[0];
            checkMax[0] = checkMax[1];
            checkMax[1] = tmp1;


            string[] check = mindate.Split('/');
            tmp1 = check[0];
            check[0] = check[1];
            check[1] = tmp1;

            textBox1.Text = check[0] + ".0" + check[1] + "." + check[2];
            textBox2.Text = checkMax[0] + ".0" + checkMax[1] + "." + checkMax[2];
            secondDate = new DateTime(Convert.ToInt32(checkMax[2]), Convert.ToInt32(checkMax[1]), Convert.ToInt32(checkMax[0]));
            firstDate = new DateTime(Convert.ToInt32(check[2]), Convert.ToInt32(check[1]), Convert.ToInt32(check[0]));

            check = textBox1.Text.Split('.');
            if (Convert.ToInt32(check[0]) == 29) { startAnalisisSec = 0; }
            else
            {
                startAnalisisSec = Convert.ToInt32(check[0]) * 86400;
            }
            string[] ch = textBox2.Text.Split('.');
            if (Convert.ToInt32(ch[0]) == 29) { endAnalisisSec = 86400; }
            else
            {
                endAnalisisSec = Convert.ToInt32(ch[0]) * 86400 + 86400;
            }

            toolStripComboBox1.Text = "Holt Winters";

        }



        private void secMinHour(double time, Label label, string str) {
            if ((time / 60) < 1)
            {
               
                label.Text = str + (int)time + " sec";
            }
            else if ((((double)time / 60 / 60) < 1) && (((double)time / 60) > 1))
            {
                time /= 60;
                
                label.Text = str + (int)time + " min";
            }
            else
            {
                time = time / 60 / 60;
                
                label.Text = str + (int)time + " hours";
            }
        }


        private void analisServer() {
           
            //////////////// Парсинг папки с логами




            for (int i = 0; i < xForPlot.Count; i+=3) {
                if (startAnalisisSec <= Convert.ToInt32(xForPlot[i])){ startAnalisisIdx = i; break; } 
            }

            for (int i = xForPlot.Count - 2; i >= 0; i-=3)
            {
                if (endAnalisisSec >= Convert.ToInt32(xForPlot[i])) { endAnalisisIdx = i; break; }
            }
            int[] dailyWork = new int[howManyDaysBetween];
            int[] scaleMass = new int[15840];
            double workingTime = 0;
            double maxworkingTime = 0;
            double minworkingTime = Convert.ToInt32(xForPlot[startAnalisisIdx + 1]) - Convert.ToInt32(xForPlot[startAnalisisIdx]);
            for (int i = 0; i < scaleMass.Length; i++) scaleMass[i] = 0;
            for (int i = 0; i < dailyWork.Length; i++) dailyWork[i] = 0;
            for (int i = startAnalisisIdx + 1; i <= endAnalisisIdx; i += 3)
            {
                if ((Convert.ToInt32(xForPlot[i]) - Convert.ToInt32(xForPlot[i - 1])) > maxworkingTime) maxworkingTime = Convert.ToInt32(xForPlot[i]) - Convert.ToInt32(xForPlot[i - 1]);
                if (((Convert.ToInt32(xForPlot[i]) - Convert.ToInt32(xForPlot[i - 1])) < minworkingTime) &&
                (Convert.ToInt32(xForPlot[i]) - Convert.ToInt32(xForPlot[i - 1])) != 0) minworkingTime = Convert.ToInt32(xForPlot[i]) - Convert.ToInt32(xForPlot[i - 1]);
                
                workingTime += Convert.ToInt32(xForPlot[i]) - Convert.ToInt32(xForPlot[i - 1]);

                for (int j = Convert.ToInt32(xForPlot[i - 1]) / 60; j < Convert.ToInt32(xForPlot[i]) / 60; j++)
                {
                    scaleMass[j] = Convert.ToInt32(xForPlot[i + 1]);
                }


            }

            
            DateTime tn = new DateTime(2016, 2, 29);
            int idx = 0;
            if (firstDate != tn) idx = 1440 * firstDate.Day;
            for (int i = 0; i < dailyWork.Length; i++)
            {
                for (int j = idx + 1440 * i; j < 1440 * i + 1440 + idx; j++)
                {
                    dailyWork[i] += scaleMass[j];

                }
                dailyWork[i] /= 1440;
            }


            //////////////////////Analis

            double[] dailyWorkExpSmooth = new double[dailyWork.Length];
            double[] dailyWorkExpSmoothLog = new double[dailyWorkExpSmooth.Length];
            double coeffReA = 0;
            double coeffReB = 0;
            double sumXY = 0;
            double sumX = 0;
            double sumY = 0;
            double sumSqX = 0;

            dailyWorkExpSmooth[0] = dailyWork[0];
            for (int i = 1; i < dailyWorkExpSmooth.Length; i++)
            {
                dailyWorkExpSmooth[i] = dailyWorkExpSmooth[i - 1] + 0.5 * (dailyWork[i] - dailyWorkExpSmooth[i - 1]);

            }

            //foreach (double i in dailyWorkExpSmooth)
            //{
            //    richTextBox1.Text += Convert.ToString(i) + "\n";
            //}

            for (int i = 0; i < dailyWorkExpSmooth.Length - 4; i++)
            {
                sumXY += Math.Log(i + 1) * dailyWorkExpSmooth[i];
                sumX += Math.Log(i + 1);
                sumY += dailyWorkExpSmooth[i];
                sumSqX += Math.Pow(Math.Log(i + 1), 2);

            }

            coeffReA = ((dailyWorkExpSmooth.Length - 4) * sumXY - sumX * sumY) / ((dailyWorkExpSmooth.Length - 4) * sumSqX - Math.Pow(sumX, 2));
            coeffReB = (sumY - coeffReA * sumX) / (dailyWorkExpSmooth.Length - 4);

            for (int i = 0; i < dailyWorkExpSmoothLog.Length; i++)
            {
                dailyWorkExpSmoothLog[i] = coeffReA * Math.Log(i + 1) + coeffReB;
            }


            ///////////////////////////
            //richTextBox1.Text = "";
            //richTextBox1.Text += "" + Convert.ToString(howManyDaysBetween) + "\n";
            //richTextBox1.Text += Convert.ToString(firstDate) + " " + Convert.ToString(secondDate) + "\n";
            //richTextBox1.Text += Convert.ToString(dailyWorkExpSmooth.Length - 4) + "\n";
            //richTextBox1.Text += Convert.ToString(coeffReA) + " " + Convert.ToString(coeffReB) + "\n sumXY " + Convert.ToString(sumXY) + " SumX" + Convert.ToString(sumX) + " SumY " + Convert.ToString(sumY) + " SumX^2 " + Convert.ToString(sumSqX) + "\n";
           

            //foreach (double i in dailyWork)
            //{
            //    richTextBox1.Text += string.Format("{0}\n", i);
            //}


            double downtime = (endAnalisisSec - startAnalisisSec - workingTime);
            double coefOfProd = workingTime / (endAnalisisSec - startAnalisisSec) * 100;

           
           // minworkingTime = minworkingTime / 60 / 60;
            
            label16.Visible = true;
            label17.Visible = true;
            label18.Visible = true;
            label19.Visible = true;
            label20.Visible = true;

          
            secMinHour(workingTime, label16, "Working time: ");
            secMinHour(downtime, label17, "Downtime: ");
            label18.Text = string.Format("Сoefficient of productivity: {0}%\n", (int)coefOfProd);
            secMinHour(maxworkingTime, label19, "Maximum worktime: ");
            secMinHour(minworkingTime, label20, "Minimum worktime: ");

            //richTextBox1.Text += string.Format("Working time: {0}\n", workingTime);
            //richTextBox1.Text += string.Format("Downtime: {0}\n", downtime);
            //richTextBox1.Text += string.Format("Сoefficient of productivity: {0}%\n", coefOfProd);
            //richTextBox1.Text += string.Format("Maximum worktime: {0} hours\n", maxworkingTime);
            //richTextBox1.Text += string.Format("Minimum worktime: {0} hours\n", minworkingTime);


            ///////////////////////////using R

            string typeOfModel = toolStripComboBox1.Text;

            
            if (dailyWork.Length <= 3) { MessageBox.Show("Time serie cannot consist from one element!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            else
            {
                REngine.SetEnvironmentVariables();
                REngine engine = REngine.GetInstance();
                // REngine requires explicit initialization.
                // You can set some parameters.
                engine.Initialize();


                double[] forR = new double[dailyWork.Length];
                for (int i = 0; i < forR.Length; i++)
                {
                    forR[i] = dailyWork[i];
                    mSW.WriteLine(Convert.ToString(forR[i]) + "    " + Convert.ToString(dailyWork[i] + "\n"));
                }



                NumericVector dd = engine.CreateNumericVector(forR);
                engine.SetSymbol("dd", dd);



                engine.Evaluate("library(FBN)");
                engine.Evaluate("fit <- medianFilter(dd, windowSize = 3)");
                engine.Evaluate("library(forecast)");
                

                //engine.Evaluate("capture.output (fit$fitted[, 1], file = 'tmp.txt')");
                //Double[] massForPlotInR = new Double [dailyWork.Length - 1];
                //String[] strForMassForPlotInR = null;
                //if (File.Exists("tmp.txt"))
                //{
                //    StreamReader rForTmp = new StreamReader("tmp.txt"); int t = 0;
                //    while (t < 4)
                //    {
                //        rForTmp.ReadLine();
                //        t++;
                //    }


                //    int ii = 0;
                //    string str = null;
                //    while ((str = rForTmp.ReadLine()) != null)
                //    {
                //      //  string str = rForTmp.ReadLine();
                //        str = str.Replace('.', ',');
                //        strForMassForPlotInR = str.Split(' ');
                //        for (int j = 1; j < strForMassForPlotInR.Length; j++)
                //        {
                //            if ((strForMassForPlotInR[j] != "") && (!(strForMassForPlotInR[j].Contains('['))))
                //            {
                //                massForPlotInR[ii] = Convert.ToDouble(strForMassForPlotInR[j]);
                //                ii++;
                //            }

                //            //i  = strForMassForPlotInR.Length - 1;
                //        }
                //    }
                //        //for (int i = 0; i < massForPlotInR.Length; i++)
                //        //{
                //        //    richTextBox2.Text += Convert.ToString(massForPlotInR[i]) + "\n";
                //        //}

                //        rForTmp.Close();

                //}
                //NumericVector fit_fitted = engine.CreateNumericVector(massForPlotInR);
                //engine.SetSymbol("fit_fitted", fit_fitted);

                if (radioButton2.Checked == true)
                {
                    engine.Evaluate("plot(c(dd, median(dd[(length(dd)-3):length(dd)])), type = 'p', col = 'blue', xlab = 'Time', ylab = 'Processor load')");
                    engine.Evaluate("lines(c(dd, NA), type = 'o')");
                    engine.Evaluate("legend('topleft', c('Initial data', 'Forecast'), col = c('black','blue'), pch = c(1,1))");
                }
                else if (radioButton1.Checked == true)
                {
                    engine.Evaluate("plot(dd, xlab = 'Days', ylab = 'Processor load', type = 'l')");
                    engine.Evaluate("lines(fit, col = 'red', type = 'l')");
                    engine.Evaluate("legend('topleft', c('Initial data', 'Model'), col = c('black','red'), pch = c(1,1))");
                    //engine.Evaluate("legend()");
                    //engine.Evaluate("lines(fit_fitted, col = 'red')");
                }

                NumericVector ddRes = engine.Evaluate("median(dd[(length(dd)-3):length(dd)])").AsNumeric();



                label21.Text = string.Format("Forecast on day {0}%", string.Join(", ", ddRes));
                label21.Visible = true;


            }
                          
                        
                        
                      
                   
              


        


        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            button1_Click_1(sender, e);
        }

        private void textBox1_Enter(object sender, EventArgs e)
        {
            monthCalendar1.Visible = true;
            whichTextbox = true;
        }

        private void textBox2_Enter(object sender, EventArgs e)
        {
            monthCalendar1.Visible = true;
            whichTextbox = false;
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
           
        }

        private void monthCalendar1_DateSelected(object sender, DateRangeEventArgs e)
        {
            if (whichTextbox == true)
            {
                string[] checkMax = maxdate.Split('/');
                string tmp = checkMax[0];
                checkMax[0] = checkMax[1];
                checkMax[1] = tmp;

                
                string[] check = mindate.Split('/');
                tmp = check[0];
                check[0] = check[1];
                check[1] = tmp;

                string[] check1 = monthCalendar1.SelectionStart.ToShortDateString().ToString().Split('.');
                
                if((Convert.ToInt32(check1[2]) - Convert.ToInt32(check[2])) < 0) { 
                    MessageBox.Show("Early date is " + check[0] + ".0" + check[1] + "." + check[2] + " !", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    textBox1.Text = check[0] + ".0" + check[1] + "." + check[2]; firstDate = new DateTime(Convert.ToInt32(check[2]), Convert.ToInt32(check[1]), Convert.ToInt32(check[0]));
                }
                else if (((Convert.ToInt32(check1[1]) - Convert.ToInt32(check[1])) < 0) && ((Convert.ToInt32(check1[2]) - Convert.ToInt32(check[2])) == 0)) { 
                        MessageBox.Show("Early date is " + check[0] + ".0" + check[1] + "." + check[2] + " !", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        textBox1.Text = check[0] + ".0" + check[1] + "." + check[2]; firstDate = new DateTime(Convert.ToInt32(check[2]), Convert.ToInt32(check[1]), Convert.ToInt32(check[0]));
                }
                else if (((Convert.ToInt32(check1[0]) - Convert.ToInt32(check[0])) < 0) && ((Convert.ToInt32(check1[1]) - Convert.ToInt32(check[1])) == 0) && ((Convert.ToInt32(check1[2]) - Convert.ToInt32(check[2])) == 0))
                {
                MessageBox.Show("Early date is " + check[0] + ".0" + check[1] + "." + check[2] + " !", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                textBox1.Text = check[0] + ".0" + check[1] + "." + check[2]; firstDate = new DateTime(Convert.ToInt32(check[2]), Convert.ToInt32(check[1]), Convert.ToInt32(check[0]));
                }



                else if ((Convert.ToInt32(check1[2]) - Convert.ToInt32(checkMax[2])) > 0)
                {
                    MessageBox.Show("Late date is " + checkMax[0] + ".0" + checkMax[1] + "." + checkMax[2] + " !", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    textBox1.Text = checkMax[0] + ".0" + checkMax[1] + "." + checkMax[2]; firstDate = new DateTime(Convert.ToInt32(checkMax[2]), Convert.ToInt32(checkMax[1]), Convert.ToInt32(checkMax[0]));
                }
                else if (((Convert.ToInt32(check1[1]) - Convert.ToInt32(checkMax[1])) > 0) && ((Convert.ToInt32(check1[2]) - Convert.ToInt32(check[2])) == 0))
                {
                    MessageBox.Show("Late date is " + checkMax[0] + ".0" + checkMax[1] + "." + checkMax[2] + " !", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    textBox1.Text = checkMax[0] + ".0" + checkMax[1] + "." + checkMax[2]; firstDate = new DateTime(Convert.ToInt32(checkMax[2]), Convert.ToInt32(checkMax[1]), Convert.ToInt32(checkMax[0]));
                }
                else if (((Convert.ToInt32(check1[0]) - Convert.ToInt32(checkMax[0])) > 0) && ((Convert.ToInt32(check1[1]) - Convert.ToInt32(checkMax[1])) == 0) && ((Convert.ToInt32(check1[2]) - Convert.ToInt32(checkMax[2])) == 0))
                {
                    MessageBox.Show("Late date is " + checkMax[0] + ".0" + checkMax[1] + "." + checkMax[2] + " !", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    textBox1.Text = checkMax[0] + ".0" + checkMax[1] + "." + checkMax[2]; firstDate = new DateTime(Convert.ToInt32(checkMax[2]), Convert.ToInt32(checkMax[1]), Convert.ToInt32(checkMax[0]));
                }

                else { textBox1.Text = monthCalendar1.SelectionStart.ToShortDateString().ToString(); firstDate = monthCalendar1.SelectionStart; }

                check1 = textBox1.Text.Split('.');
                if (Convert.ToInt32(check1[0]) == 29) { startAnalisisSec = 0; }
                else {
                    startAnalisisSec = Convert.ToInt32(check1[0]) * 86400;
                }
                    
            }
            if (whichTextbox == false) {
                string[] checkMax = maxdate.Split('/');
                string tmp = checkMax[0];
                checkMax[0] = checkMax[1];
                checkMax[1] = tmp;


                string[] check = mindate.Split('/');
                tmp = check[0];
                check[0] = check[1];
                check[1] = tmp;

                string[] check1 = monthCalendar1.SelectionStart.ToShortDateString().ToString().Split('.');

                if ((Convert.ToInt32(check1[2]) - Convert.ToInt32(check[2])) < 0)
                {
                    MessageBox.Show("Early date is " + check[0] + ".0" + check[1] + "." + check[2] + " !", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    textBox2.Text = check[0] + ".0" + check[1] + "." + check[2]; secondDate = new DateTime(Convert.ToInt32(check[2]), Convert.ToInt32(check[1]), Convert.ToInt32(check[0]));
                }
                else if (((Convert.ToInt32(check1[1]) - Convert.ToInt32(check[1])) < 0) && ((Convert.ToInt32(check1[2]) - Convert.ToInt32(check[2])) == 0))
                {
                    MessageBox.Show("Early date is " + check[0] + ".0" + check[1] + "." + check[2] + " !", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    textBox2.Text = check[0] + ".0" + check[1] + "." + check[2]; secondDate = new DateTime(Convert.ToInt32(check[2]), Convert.ToInt32(check[1]), Convert.ToInt32(check[0]));
                }
                else if (((Convert.ToInt32(check1[0]) - Convert.ToInt32(check[0])) < 0) && ((Convert.ToInt32(check1[1]) - Convert.ToInt32(check[1])) == 0) && ((Convert.ToInt32(check1[2]) - Convert.ToInt32(check[2])) == 0))
                {
                    MessageBox.Show("Early date is " + check[0] + ".0" + check[1] + "." + check[2] + " !", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    textBox2.Text = check[0] + ".0" + check[1] + "." + check[2]; secondDate = new DateTime(Convert.ToInt32(check[2]), Convert.ToInt32(check[1]), Convert.ToInt32(check[0]));
                }



                else if ((Convert.ToInt32(check1[2]) - Convert.ToInt32(checkMax[2])) > 0)
                {
                    MessageBox.Show("Late date is " + checkMax[0] + ".0" + checkMax[1] + "." + checkMax[2] + " !", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    textBox2.Text = checkMax[0] + ".0" + checkMax[1] + "." + checkMax[2]; secondDate = new DateTime(Convert.ToInt32(checkMax[2]), Convert.ToInt32(checkMax[1]), Convert.ToInt32(checkMax[0]));
                }
                else if (((Convert.ToInt32(check1[1]) - Convert.ToInt32(checkMax[1])) > 0) && ((Convert.ToInt32(check1[2]) - Convert.ToInt32(check[2])) == 0))
                {
                    MessageBox.Show("Late date is " + checkMax[0] + ".0" + checkMax[1] + "." + checkMax[2] + " !", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    textBox2.Text = checkMax[0] + ".0" + checkMax[1] + "." + checkMax[2]; secondDate = new DateTime(Convert.ToInt32(checkMax[2]), Convert.ToInt32(checkMax[1]), Convert.ToInt32(checkMax[0]));
                }
                else if (((Convert.ToInt32(check1[0]) - Convert.ToInt32(checkMax[0])) > 0) && ((Convert.ToInt32(check1[1]) - Convert.ToInt32(checkMax[1])) == 0) && ((Convert.ToInt32(check1[2]) - Convert.ToInt32(checkMax[2])) == 0))
                {
                    MessageBox.Show("Late date is " + checkMax[0] + ".0" + checkMax[1] + "." + checkMax[2] + " !", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    textBox2.Text = checkMax[0] + ".0" + checkMax[1] + "." + checkMax[2]; secondDate = new DateTime(Convert.ToInt32(checkMax[2]), Convert.ToInt32(checkMax[1]), Convert.ToInt32(checkMax[0]));
                }

                else { textBox2.Text = monthCalendar1.SelectionStart.ToShortDateString().ToString(); secondDate = monthCalendar1.SelectionStart; }

                string[] ch = textBox2.Text.Split('.');
                if (Convert.ToInt32(ch[0]) == 29) { endAnalisisSec = 86400; }
                else
                {
                    endAnalisisSec = Convert.ToInt32(ch[0]) * 86400 + 86400;
                }

            }

            


            monthCalendar1.Visible = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            isFirstOpen = false;
            if (secondDate >= firstDate)
            {
                howManyDaysBetween = Convert.ToInt32((secondDate - firstDate).TotalDays) + 1;
                //richTextBox1.Text = "";
                analisServer();
            }
            else MessageBox.Show("Wrong sequence of dates!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

                    }

        private void tabPage3_Click(object sender, EventArgs e)
        {
            monthCalendar1.Visible = false;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if(!isFirstOpen)
            button4_Click(sender, e);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!isFirstOpen)
            button4_Click(sender, e);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (!isFirstOpen)
            button4_Click(sender, e);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (!isFirstOpen)
            button4_Click(sender, e);
        }

        private void toolStripComboBox1_TextChanged(object sender, EventArgs e)
        {
            if (!isFirstOpen)
            button4_Click(sender, e);
        }

        private void toolStripComboBox1_DropDownStyleChanged(object sender, EventArgs e)
        {
            if (!isFirstOpen)
                button4_Click(sender, e);
        }

        private void toolStripComboBox1_DropDownClosed(object sender, EventArgs e)
        {
            if (!isFirstOpen)
                button4_Click(sender, e);
        }



        
    }
}
