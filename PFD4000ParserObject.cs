using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;





namespace PFD4000 {




    //-------------------------------------------------------------------------------------------------
    public class PFD4000ParserObject {

        public enum Codes {
            OkCode = 290,
            WrongPreFix = 299,
            WrongQSKey = 299,
            ErrorCode = 299,
            LogErrorCode = 299,
            SqlConnectionError = 399,
        } ;

        public delegate void PoliCustomEventHandler ( object sender, PoliCustomEventArgs e );
        
        public event PoliCustomEventHandler PoliRaiseCustomEvent;





        /// <summary>
        /// 
        /// </summary>
        public PFD4000ParserObject () { 
        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected void OnPoliRaiseCustomEvent ( object sender, PoliCustomEventArgs args ) {
            if (PoliRaiseCustomEvent == null) return;
            PoliRaiseCustomEvent(sender, args);
        }
        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adi"></param>
        /// <param name="msg"></param>
        private void raiseMessage ( int id, string adi, string msg ) {
            PoliCustomEventArgs args = new PoliCustomEventArgs();
            args.ID = id;
            args.adi = adi;
            args.mesaj = "\r\n"+msg+"\r\n";
            OnPoliRaiseCustomEvent(this, args);
        }
        //---------------------------------------------------------------------------------------------
        /// <summary>
        /// Cihazdan gelen bilgi burda parse edilir ve cihaza verilecek cevap PoliWSSendDataObject tipinde hazırlanır ve geri döndürülür.
        /// </summary>
        /// <param name="cmdComKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public PoliWSSendDataObject parseData ( string cmdComKey, string data ) {

            PoliWSSendDataObject retObj = new PoliWSSendDataObject();
            retObj.listData = new ArrayList();
            retObj.listHeaders = new Dictionary<string, string>();

            retObj.statusCode = (int)Codes.OkCode;
            retObj.errorMsg = "";

            try {

                if (data == null) {
                    retObj.statusCode = (int)Codes.ErrorCode;
                    throw new Exception("Query string yok!");
                }
                string[] dz1 = data.Split(new char[] { ':' });
                if (dz1 == null) {
                    retObj.statusCode = (int)Codes.ErrorCode;
                    throw new Exception("Query string yok!");
                }
                if (dz1.Length < 2) {
                    retObj.statusCode = (int)Codes.ErrorCode;
                    throw new Exception("Query string yok!!");
                }

                string commandStr = dz1[0];

                string[] dz2 = dz1[1].Split(new char[] { ',' });
                if (dz2 == null) {
                    retObj.statusCode = (int)Codes.ErrorCode;
                    throw new Exception("Query string yok!!!!");
                }
                if (dz2.Length < 1) {
                    throw new Exception("Query string yok!!!!!");
                }

                string imeiPrm = "";
                string tomidPrm = "";
                string versionPrm = "";
                string degerPrm = "";
                string tarihPrm = "";

                string remoteHost = "ismail";
                if (String.IsNullOrEmpty(remoteHost)) remoteHost = "?";

                TerminalDataObject termDataObj = new TerminalDataObject();
                termDataObj.commandStr = commandStr;

                switch (commandStr) {
                    case "TOM": /* Cihaza belirli sürelerde bir TOM komutu ile sorgu yapar. Bu komuta ayrıca, T headerı ile server tarihsaati saniye cinsinden gönderilmelidir. */
                        tomidPrm = dz2[0];
                        degerPrm = dz2[1];
                        imeiPrm = dz2[2];
                        versionPrm = dz2[3];

                        termDataObj.kartId = tomidPrm;
                        termDataObj.macId = imeiPrm;
                        termDataObj.termYon = degerPrm;
                        termDataObj.version = versionPrm;
                        retObj.terminalData = termDataObj;

                        // acc2000ResponseToTerminal(retObj, commandStr, tomidPrm, imeiPrm, dsCmdData);
                        // acc2000CheckYetkiGonderimi(retObj, imeiPrm);

                        break;
                    case "SALE":  /* Cihaza kart okutuldu. Online çalışma. */
                        // SALE:EC000601,0,0050C2235000,0,0,0
                        tomidPrm = dz2[0];
                        degerPrm = dz2[1];
                        imeiPrm = dz2[2];
                        versionPrm = dz2[3];

                        termDataObj.kartId = tomidPrm;
                        termDataObj.macId = imeiPrm;
                        termDataObj.termYon = degerPrm;
                        termDataObj.version = versionPrm;
                        retObj.terminalData = termDataObj;


                        // acc2000ResponseToTerminal(retObj, commandStr, tomidPrm, imeiPrm, dsCmdData);
                        break;
                    case "OSALE": /* Offline modda oluşan kayıt aktarımı.  */
                        tomidPrm = dz2[0];
                        degerPrm = dz2[1];
                        imeiPrm = dz2[2];
                        tarihPrm = dz2[3];
                        versionPrm = dz2[4];

                        termDataObj.kartId = tomidPrm;
                        termDataObj.macId = imeiPrm;
                        termDataObj.termYon = degerPrm;
                        termDataObj.version = versionPrm;
                        termDataObj.tarih = tarihPrm;
                        retObj.terminalData = termDataObj;

                        // izinPrm      = dz2[5];
                        // aktarinPrm   = dz2[6];
                        // OSALE:00C884C7,1,0050C2235DA7,1327923592,0,0
                        // acc2000ResponseToTerminal(retObj, commandStr, tomidPrm, imeiPrm, dsCmdData);
                        break;
                    case "PER":  /* Cihaz offline çalışma için personel kart listesini istemektedir. Kart listesi cihaza gönderilecek. */
                        imeiPrm = dz2[0];
                        tomidPrm = "";
                        
                        termDataObj.kartId = tomidPrm;
                        termDataObj.macId = imeiPrm;
                        
                        retObj.terminalData = termDataObj;

                        // acc2000ResponseToTerminal(retObj, commandStr, tomidPrm, imeiPrm, dsCmdData);
                        break;
                    case "CANCEL":
                        tomidPrm = dz2[0];
                        imeiPrm = dz2[1];
                        termDataObj.kartId = tomidPrm;
                        termDataObj.macId = imeiPrm;
                        retObj.terminalData = termDataObj;

                        break;

                    default :
                        retObj.statusCode = (int)Codes.ErrorCode;
                        retObj.errorMsg = "Geçersiz komut!!!";
                        raiseMessage(-1, "", "Geçersiz komut!!!");
                        
                        termDataObj.commandStr = "";

                        break;
                }

            } catch (Exception ex) {
                retObj.statusCode = (int)Codes.ErrorCode;
                retObj.listData = new ArrayList();
                retObj.listHeaders = new Dictionary<string, string>();
                retObj.errorMsg = ex.Message;
                raiseMessage( -1, "", ex.Message  );
            }
            return retObj;

        }
        //---------------------------------------------------------------------------------------------

    }
    //-------------------------------------------------------------------------------------------------

}
