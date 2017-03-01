using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



namespace PFD4000 {

    
    //-------------------------------------------------------------------------------------------------
    public class PoliCustomEventArgs : EventArgs {

        public int ID { get; set; }
        public string adi { get; set; }
        public string mesaj { get; set; }


        //---------------------------------------------------------------------------------------------
        public PoliCustomEventArgs () {
            ID = -1;
            adi = "";
            mesaj = "";
        }
        //---------------------------------------------------------------------------------------------

    }
    //-------------------------------------------------------------------------------------------------
}


