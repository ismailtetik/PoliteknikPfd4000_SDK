using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PFD4000 {
    public class TermMsgColor {
        public string kod { get; set; }
        public string ad { get; set; }
        public int id { get; set; }
        public override string  ToString() {
 	        return ad; 
        }
    }
}
