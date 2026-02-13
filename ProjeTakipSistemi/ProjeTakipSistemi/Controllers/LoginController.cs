using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjeTakipSistemi.Models;

namespace ProjeTakipSistemi.Controllers
{
    public class LoginController : Controller
    {
        ptsEntities entity = new ptsEntities();
        // GET: Login
        public ActionResult Index()
        {
            TempData.Clear();
            ViewBag.mesaj = null;
            return View();
        }
        [HttpPost]
        public ActionResult Index(int kullaniciAd,string parola)
        {
            var ogrenci = (from o in entity.Ogrenciler where o.ogrID == kullaniciAd && o.ogrSifre == parola select o).FirstOrDefault();
            if (ogrenci != null) {

                Session["OgrenciAdSoyad"] = ogrenci.ogrAd + " " + ogrenci.ogrSoyad;
                Session["OgrenciID"] = ogrenci.ogrID;
                Session["OgrenciBolumID"] = ogrenci.ogrBolum;
                Session["OgrenciYetkiTuru"] = ogrenci.yetkiTuru;

                return RedirectToAction("Index", "Ogrenci");
            }
            var yonetici = (from y in entity.OgretimElemanlari
                            where y.SicilNo == kullaniciAd && y.ogrElmSifre == parola
                            select y).FirstOrDefault();

            if (yonetici != null)
            {
                Session["OgretimElemanıAdSoyad"] = yonetici.ogrElmAd + " " + yonetici.ogrElmSoyad;
                Session["OgretimELemanID"] = yonetici.SicilNo;
                Session["OgretimElemanıBolumID"] = yonetici.ogrElmBolum;
                Session["OgretimElemaniYetkiTuru"] = yonetici.yetkiTuru;

                return RedirectToAction("Index", "Yonetici");
            }

            var admin = (from a in entity.Admin where a.adminID == kullaniciAd && a.adminSifre == parola select a).FirstOrDefault();
            if (admin != null)
            {
                Session["AdminAdSoyad"] = admin.adminAdi + " " + admin.adminSoyadi;
                Session["AdminID"] = admin.adminID;
                Session["AdminYetkiTuru"] = admin.adminYetki;

                return RedirectToAction("Index", "SistemYoneticisi");
            }
            else 
            {
                ViewBag.mesaj = "Kullanıcı adı ya da şifre yanlış";
                return View(); 
            
            } 
                
                
        } 
    }
}