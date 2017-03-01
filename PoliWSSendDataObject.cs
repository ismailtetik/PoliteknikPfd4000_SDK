using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;



namespace PFD4000 {

    
    public class PoliWSSendDataObject {

        public int statusCode { get; set; }
        public ArrayList listData { get; set; }
        public Dictionary<string, string> listHeaders { get; set; }
        public TerminalDataObject terminalData { get; set; }

        public string errorMsg { get; set; }

        //---------------------------------------------------------------------------------------------
        public PoliWSSendDataObject () {
            statusCode = 299;
            errorMsg = "";
            listData = new ArrayList();
            terminalData = new TerminalDataObject();
            listHeaders = new Dictionary<string, string>();
        }
        //---------------------------------------------------------------------------------------------
        //---------------------------------------------------------------------------------------------
    }

}
