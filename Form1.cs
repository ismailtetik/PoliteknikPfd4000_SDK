using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO.Ports;
using System.Collections;






namespace PFD4000 {


    //-------------------------------------------------------------------------------------------------
    public partial class Form1 : Form {

        // private HttpListener tcpListener = new HttpListener();
        private Thread threadTcpListener;

        private TcpListener tcpListener2 = null; // new TcpListener(localAddress, portNumber);

        private DataTable dtParams = new DataTable();

        /* Cihazın bağlanacağı port numarası */
        private int tcpPortValue = 85 ; // 502;
        // private int tcpPortValue = 9923; 

        private bool maiThreadContinue = false;
        private PFD4000ParserObject pfdParser = new PFD4000ParserObject();

        private ArrayList listReceivedKartData = new ArrayList();

        private SerialPort serialPort = new SerialPort();


        //---------------------------------------------------------------------------------------------
        public Form1 () {
            try {

                InitializeComponent();

                // byte [] dz =  Encoding.GetEncoding( 1254 ).GetBytes("Öö");

                btnStop.Enabled = false;

            } catch ( Exception ex ) {
                MessageBox.Show ( ex.Message ) ; 
            }
        }
        //---------------------------------------------------------------------------------------------
        private void btnExit_Click ( object sender, EventArgs e ) {
            try {
                maiThreadContinue = false;
                this.Close();
            } catch (Exception ex) {
                appendMsg("\r\n\r\n HATA \r\n" + ex.Message + "\r\n\r\n");
            }
        }
        //---------------------------------------------------------------------------------------------
        private void btnClear_Click ( object sender, EventArgs e ) {
            txtMsg.Text = "";
        }
        //---------------------------------------------------------------------------------------------
        public void appendMsg ( string text ) {
            try {
                if (this.txtMsg.InvokeRequired) {
                    this.txtMsg.BeginInvoke(
                        new MethodInvoker( delegate( ) { appendMsg( text ); } ) );
                } else {
                    this.txtMsg.AppendText( text );
                }
            } catch (Exception) { } 
        }
        //---------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------
        private void btnStart_Click ( object sender, EventArgs e ) {
            try {

                appendMsg(" Servis başlıyor...\r\n");

                pfdParser.PoliRaiseCustomEvent += new PFD4000ParserObject.PoliCustomEventHandler(pfdParser_PoliRaiseCustomEvent);

                maiThreadContinue = true;
                threadTcpListener = new Thread(tcpListenResponseTerminal2);
                threadTcpListener.Start();

                appendMsg("\r\n\r\n Servis başladı... \r\n\r\n");


                btnStart.Enabled = false;
                btnStop.Enabled = true;

            } catch (Exception ex) {
                appendMsg("\r\n\r\n HATA -2 : \r\n" + ex.Message + "\r\n\r\n");
            }
        }
        //---------------------------------------------------------------------------------------------
        private void btnStop_Click ( object sender, EventArgs e ) {
            try {
                
                maiThreadContinue = false;
                threadTcpListener.Abort();
                
                btnStart.Enabled = true;
                btnStop.Enabled = false;
                
            } catch (Exception ex) {
                appendMsg("\r\n\r\n HATA -3 : \r\n" + ex.Message + "\r\n\r\n");
            }
        }
        //---------------------------------------------------------------------------------------------
        private void Form1_FormClosing ( object sender, FormClosingEventArgs e ) {
            try {

            } catch (Exception ex) {
                appendMsg("\r\n\r\n HATA -4 : \r\n" + ex.Message + "\r\n\r\n");
            }
        }
        //---------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------
        public void tcpListenResponseTerminal2 () {
            try {

                appendMsg("\r\n start tcpListenResponseTerminal2 \r\n");

                System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
                IPAddress localAddress = IPAddress.Any;
                tcpListener2 = new TcpListener(localAddress, tcpPortValue);
                tcpListener2.Start();
                int dakika = DateTime.Now.Minute;

                while (maiThreadContinue) {
                    try {

                        Socket socketTcp = null;
                        Thread thread2 = null;
                        if (socketTcp == null) {
                            /* Terminalin sorgulama yapması bekleniyor */
                            socketTcp = tcpListener2.AcceptSocket();
                            thread2 = new Thread(delegate() {
                                startSocketProcess(socketTcp, thread2);
                            });

                            thread2.Start();

                        }
                        Thread.Sleep(100);
                    } catch (Exception eexx66) {
                        appendMsg( "\r\n\r\n"+eexx66.Message+"\r\n\r\n" );
                    }
                }
            } catch (Exception ex) {
                appendMsg("\r\n\r\n" + ex.Message + "\r\n\r\n");
            }
        }
        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Terminal sorgusu işlenecek
        /// </summary>
        /// <param name="socketTcp"></param>
        /// <param name="threadTcp"></param>
        private void startSocketProcess ( Socket socketTcp, Thread threadTcp ) {
            bool continuProcess = true;

            appendMsg("\r\n\r\n ******* startSocketProcess ******* \r\n\r\n");

            long startTick = DateTime.Now.Ticks;
            // double saniyeTick = 10000000;
            // double gsn = (stopTick - startTick) / saniyeTick;

            int timeOut2 = 1000 * 60 * 3;

            socketTcp.SendTimeout = timeOut2;
            socketTcp.ReceiveTimeout = timeOut2;

            // socketTcp.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 1000);

            while ( continuProcess ) {

                /*
                try {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                } catch (Exception) { }
                 * */

                try {

                    // long stopTick = DateTime.Now.Ticks ;
                    // double gsn = ( stopTick - startTick ) / saniyeTick ;
                    /*
                    if ( gsn > timeOut ) {
                        writeToLog(" ******* time out ******* ");
                    }
                    */

                    // if ((!socketTcp.Connected) || (gsn > timeOut)) {
                    if (!socketTcp.Connected) {

                        socketTcp.Disconnect(true);
                        try { socketTcp.Shutdown(SocketShutdown.Both); } catch (Exception) { }
                        try { socketTcp.Close(); } catch (Exception) { }
                        try { socketTcp = null; } catch (Exception) { }
                        try { threadTcp.Abort(); } catch (Exception) { }
                        try { threadTcp = null; } catch (Exception) { }
                        continuProcess = false;
                        continue;
                    }

                    Byte[] received = new Byte[512];

                    // startTick = DateTime.Now.Ticks;
                    Thread.Sleep(50);
                    int bytesReceived = socketTcp.Receive(received, received.Length, 0);
                    int avail = socketTcp.Available;
                    if (bytesReceived <= 0) {
                        // appendMsg( "bytesReceived <= 0" );
                        continue;
                    }

                    byte[] dzReceiveData = new byte[bytesReceived];
                    for (int i = 0; i < dzReceiveData.Length; i++) {
                        dzReceiveData[i] = (byte)received[i];
                    }

                    String dataReceived = System.Text.Encoding.ASCII.GetString(received);
                    if ( dataReceived == null ) dataReceived = "NULL";
                    appendMsg( "\r\n\r\n RAW DATA : #" + dataReceived + "#    - datalen : " + dataReceived.Length );
                    // continue;
                    
                    int indexGet2 = dataReceived.ToUpper().IndexOf("GET");
                    if (indexGet2 < 0) {
                        appendMsg("\r\n\r\n ******* GET metodu degil!!! ******* \r\n\r\n");
                        continue;
                        // indexGet2 = 0;
                    } else {
                        indexGet2 = +3;
                    }

                    int indexHttp2 = dataReceived.ToUpper().IndexOf("HTTP");
                    if (indexHttp2 < 0) {
                        appendMsg("\r\n\r\n Gelen Veri hatalı!!! ( HTTP not found! ) \r\n\r\n");
                        continue;
                    }

                    string receivedDataRaw = dataReceived.Substring(indexGet2, (indexHttp2 - indexGet2));
                    receivedDataRaw = receivedDataRaw.Trim();
                    // Console.WriteLine("RAW - gelen data : #" + receivedDataRaw+"#");

                    appendMsg( "\r\n\r\n Remote IP & Port : \r\n " + socketTcp.RemoteEndPoint.ToString() + "" );
                    appendMsg("\r\n Raw Data : \r\n " + receivedDataRaw + " \r\n");

                    appendMsg("\r\n " + receivedDataRaw + " \r\n");

                    string sanalDizinAdi = "";
                    string[] dz1 = receivedDataRaw.Split(new char[] { '?' });
                    if (dz1.Length < 2) {
                        appendMsg("\r\n\r\n Hata : 74 \r\n Query string eksik!!! \r\n");
                        continue;
                    }
                    sanalDizinAdi = dz1[0].Replace("/", "");
                    dz1 = dz1[1].Split(new char[] { '=' });
                    if (dz1.Length < 2) {
                        appendMsg("\r\n\r\n Hata : 75 \r\n Query string eksik!!! \r\n");
                        continue;
                    }

                    string cmdName = dz1[0];
                    string dataQuery = dz1[1];


                    appendMsg("\r\n\r\n Command : '" + cmdName + "' \r\n");
                    appendMsg(" Data : '" + dataQuery + "' \r\n");



                    PoliWSSendDataObject retObj = pfdParser.parseData( cmdName, dataQuery ); //parseAccess2000SistemiData(cmdName, dataQuery);

                    byte[] dzEnter = { 0x0D, 0x0A };

                    string msg = "";
                    if (retObj == null ) {
                        msg = "HTTP/1.1 299 ";
                        socketTcp.Send(Encoding.GetEncoding(1254).GetBytes(msg), msg.Length, 0);
                        socketTcp.Send(dzEnter, dzEnter.Length, 0);

                        msg = "Content-Length: 0";
                        socketTcp.Send( Encoding.GetEncoding( 1254 ).GetBytes( msg ), msg.Length, 0 );
                        socketTcp.Send(dzEnter, dzEnter.Length, 0);
                        socketTcp.Send(dzEnter, dzEnter.Length, 0);
                        appendMsg( "\r\n Data parse edilemedi !!! ( NULL ) \r\n" );
                        continue;
                    }

                    if (retObj.statusCode != 290) {
                        msg = "HTTP/1.1 299 ";
                        socketTcp.Send( Encoding.GetEncoding( 1254 ).GetBytes( msg ), msg.Length, 0 );
                        socketTcp.Send(dzEnter, dzEnter.Length, 0);

                        msg = "Content-Length: 0";
                        socketTcp.Send( Encoding.GetEncoding( 1254 ).GetBytes( msg ), msg.Length, 0 );
                        socketTcp.Send(dzEnter, dzEnter.Length, 0);
                        socketTcp.Send(dzEnter, dzEnter.Length, 0);
                        appendMsg("\r\n Status Kod : "+retObj.statusCode+" !!!  \r\n");
                        continue;
                    }


                    appendMsg("\r\n\r\n");
                    if (!String.IsNullOrEmpty(retObj.terminalData.macId)) {
                        appendMsg("Mac ID : " + retObj.terminalData.macId+"\r\n");
                        appendMsg("Kart ID : " + retObj.terminalData.kartId + "\r\n");
                        appendMsg("Reader Yön : " + retObj.terminalData.termYon + "\r\n");
                        appendMsg( "Device IP : " + (socketTcp.RemoteEndPoint.ToString().Split( new char[] {':'} ))[0] + "\r\n" );
                        appendMsg( "Device Tcp Port : " + ( socketTcp.RemoteEndPoint.ToString().Split( new char[] { ':' } ) )[1] + "\r\n" );
                    }

                    msg = "HTTP/1.1 " + retObj.statusCode + " ";
                    socketTcp.Send( Encoding.GetEncoding( 1254 ).GetBytes( msg ), msg.Length, 0 );
                    socketTcp.Send(dzEnter, dzEnter.Length, 0);

                    foreach (string key in retObj.listHeaders.Keys) {
                        string hValue = retObj.listHeaders[key];
                        msg = "" + key + ": " + hValue + "";
                        socketTcp.Send( Encoding.GetEncoding( 1254 ).GetBytes( msg ), msg.Length, 0 );
                        socketTcp.Send(dzEnter, dzEnter.Length, 0);
                    }

                    //-----------------
                    /* T header'ı ile 1970.01.01 den bugüne kadar geçen zaman sn cinsinden terminale geçiliyor. Bu T headerı cihazın her sorgusunda gönderilmelidir. */
                    long l = DateTime.Now.Ticks / 10000000;
                    l -= (new DateTime(1970, 1, 1).Ticks / 10000000);
                    msg = "T: " + l.ToString();
                    socketTcp.Send(Encoding.GetEncoding(1254).GetBytes(msg), msg.Length, 0);
                    socketTcp.Send(dzEnter, dzEnter.Length, 0);
                    //-----------------

                    if ( !String.IsNullOrEmpty( txtMsgAdSoyad.Text ) ) {
                        msg = "MSG: " + txtMsgAdSoyad.Text;
                        socketTcp.Send( Encoding.GetEncoding( 1254 ).GetBytes( msg ), msg.Length, 0 );
                        socketTcp.Send( dzEnter, dzEnter.Length, 0 );
                    }

                    if (cbRole1.Checked) {
                        // Röle1 çektirilecek. ROLE1 headerın değeri rölenin çekme süresini değiştirir. 
                        // 1 : 50 ms 
                        msg = "ROLE" + ": 10"; //  ( deger * 50ms  );
                        socketTcp.Send( Encoding.GetEncoding( 1254 ).GetBytes( msg ), msg.Length, 0 );
                        socketTcp.Send(dzEnter, dzEnter.Length, 0);
                    }

                    if (cbRole2.Checked) {
                        // Röle2 çektirilecek. ROLE2 headerın değeri rölenin çekme süresini değiştirir. 
                        msg = "ROLE2" + ": 10";  // ROLE2
                        socketTcp.Send( Encoding.GetEncoding( 1254 ).GetBytes( msg ), msg.Length, 0 );
                        socketTcp.Send( dzEnter, dzEnter.Length, 0 );
                    }


                    if (retObj.listData.Count <= 0) {
                        msg = "Content-Length: 0";
                        socketTcp.Send( Encoding.GetEncoding( 1254 ).GetBytes( msg ), msg.Length, 0 );
                        socketTcp.Send(dzEnter, dzEnter.Length, 0);
                        socketTcp.Send(dzEnter, dzEnter.Length, 0);
                        continue;
                    }

                    string sendData = retObj.listData[0].ToString();
                    byte[] dzData = new byte[sendData.Length / 2];
                    byte a1;
                    byte a2;

                    for (int i = 0; i < dzData.Length; i++) {
                        a1 = (byte)sendData[2 * i];
                        a2 = (byte)sendData[2 * i + 1];
                        if (a1 > '9') a1 -= 55; else a1 -= 48;
                        if (a2 > '9') a2 -= 55; else a2 -= 48;
                        a1 *= 16;
                        a1 += a2; 
                        dzData[i] = a1; 
                    } 
                    // byte[] dzData = ASCIIEncoding.ASCII.GetBytes( sendData ) ; 

                    msg = "Content-Type: text/html";
                    socketTcp.Send( Encoding.GetEncoding( 1254 ).GetBytes( msg ), msg.Length, 0 ); 
                    socketTcp.Send(dzEnter, dzEnter.Length, 0); 

                    msg = "Content-Length: " + dzData.Length + "";
                    socketTcp.Send( Encoding.GetEncoding( 1254 ).GetBytes( msg ), msg.Length, 0 ); 
                    socketTcp.Send(dzEnter, dzEnter.Length, 0); 
                    socketTcp.Send(dzEnter, dzEnter.Length, 0); 

                    if (dzData.Length > 0) { 
                        socketTcp.Send(dzData, dzData.Length, 0); 
                        continue; 
                    } else { 
                        appendMsg("\r\n  acc - kart listesi boş!!! \r\n"); 
                    } 

                } catch ( Exception ex ) { 
                    appendMsg("\r\n\r\n  "+ex.Message+" \r\n"); 
                    // if (socketTcp != null)  
                    { 
                        // if (!socketTcp.Connected)  
                        { 
                            try { socketTcp.Shutdown ( SocketShutdown.Both ) ; } catch ( Exception ) { } 
                            try { socketTcp.Close() ; } catch ( Exception ) { } 
                            try { socketTcp = null  ; } catch ( Exception ) { } 
                            try { threadTcp.Abort() ; } catch ( Exception ) { } 
                            try { threadTcp = null  ; } catch ( Exception ) { } 
                            continuProcess  = false ; 
                        }
                    }
                    // else 
                    {
                        continuProcess = false;
                    }

                }

                /*
                try {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                } catch (Exception) { }
                * */

            }

            try {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            } catch (Exception) { }


            appendMsg("\r\n\r\n  !!!!!!!!!  SOCKET CLOSED !!!!!!!!! \r\n");

        }
        //---------------------------------------------------------------------------------------------
        protected void pfdParser_PoliRaiseCustomEvent ( object sender, PoliCustomEventArgs e ) {
            try {
                appendMsg("\r\nParser event \r\n" + e.mesaj);
            } catch (Exception ex) {
                appendMsg("\r\n\r\n HATA -2 : \r\n" + ex.Message + "\r\n\r\n");
            }
        }
        //---------------------------------------------------------------------------------------------

    }
    //-------------------------------------------------------------------------------------------------
}
//-----------------------------------------------------------------------------------------------------



