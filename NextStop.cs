using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UlasimRotaPlanlama.Models.Arac
 {
     public class NextStop
     {
         public string StopId { get; set; }   
         public double Mesafe { get; set; }   
         public int Sure { get; set; }        
         public double Ucret { get; set; }    
 
         public NextStop(string stopId, double mesafe, int sure, double ucret)
         {
             StopId = stopId;
             Mesafe = mesafe;
             Sure = sure;
             Ucret = ucret;
         }
     }
 }