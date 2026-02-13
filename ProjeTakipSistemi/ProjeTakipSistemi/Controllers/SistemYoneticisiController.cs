using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjeTakipSistemi.Models;
using System.Globalization;

namespace ProjeTakipSistemi.Controllers
{
    public class SistemYoneticisiController : Controller
    {
        ptsEntities entity = new ptsEntities();

        // GET: SistemYoneticisi
        public ActionResult Index()
        {
            int yetkiTurId = Convert.ToInt32(Session["AdminYetkiTuru"]);
            if (yetkiTurId == 3)
            {
                return View();
            }
            else 
            {
                return RedirectToAction("Index", "Login"); 
            }
               
        }

        public ActionResult Bolum()
        {
            int yetkiTurId = Convert.ToInt32(Session["AdminYetkiTuru"]);
            if (yetkiTurId == 3)
            {
                var bolumler = (from b in entity.Bolumler select b).ToList();
                return View(bolumler);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult Olustur()
        {
            int yetkiTurId = Convert.ToInt32(Session["AdminYetkiTuru"]);
            if (yetkiTurId == 3)
            {
                var bolumler = (from b in entity.Bolumler select b).ToList();
                return View(bolumler);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
        [HttpPost]
        public ActionResult Olustur(string bolumAd)
        {
            Bolumler yeniBolum = new Bolumler();
            string yeniBolumAd = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(bolumAd);
            yeniBolum.BolumAd = yeniBolumAd;

            entity.Bolumler.Add(yeniBolum);
            entity.SaveChanges();
            return RedirectToAction("Bolum");
        }

        public ActionResult Guncelle(int id)
        {
            int yetkiTurId = Convert.ToInt32(Session["AdminYetkiTuru"]);
            if (yetkiTurId == 3)
            {
                var bolum = (from b in entity.Bolumler where b.BolumID ==id select b).FirstOrDefault();
                return View(bolum);
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        [HttpPost]
        public ActionResult Guncelle(FormCollection fc)
        {
            int bolumId = Convert.ToInt32(fc["BolumID"]);
            string yeniBolumAdi = fc["BolumAd"];

            var bolum = (from b in entity.Bolumler where b.BolumID ==bolumId select b).FirstOrDefault();

            bolum.BolumAd = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(yeniBolumAdi);
            entity.SaveChanges();
            return RedirectToAction("Bolum");

        }

        public ActionResult Sil(int id)
        {
            int yetkiTurId = Convert.ToInt32(Session["AdminYetkiTuru"]);
            if (yetkiTurId == 3)
            {
                var bolum = (from b in entity.Bolumler where b.BolumID == id select b).FirstOrDefault();
                entity.Bolumler.Remove(bolum);
                entity.SaveChanges();
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }
    }
}