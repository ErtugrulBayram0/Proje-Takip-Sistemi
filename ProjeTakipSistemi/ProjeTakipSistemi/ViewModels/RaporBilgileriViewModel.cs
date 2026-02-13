using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjeTakipSistemi.ViewModels
{
    public class RaporBilgileriViewModel
    {
        public string ProjeAd { get; set; }
        public string DosyaAdi { get; set; }
        public string DosyaYolu {  get; set; }
        public int RaporID { get; set; }
        public string OgrenciAdSoyad {  get; set; }
        public DateTime? YuklenenTarih {  get; set; }

    }
}