using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace PFD4000 {

    
    public class TerminalDataObject {


        public string commandStr { get; set; }
        public string kartId { get; set; }
        public string macId { get; set; }
        public string version { get; set; }
        public string termYon { get; set; }
        public string tarih { get; set; }

        public TerminalDataObject () {
            commandStr = "";
            kartId = "";
            macId = "";
            version = "";
            termYon = "0";
            tarih = "01.01.01";
        }

    }
}


