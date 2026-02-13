using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Mvc;
using ProjeTakipSistemi.Models;
using ProjeTakipSistemi.ViewModels;
using System.IO;
using System.Data.Entity;

namespace ProjeTakipSistemi.Controllers
{
    public class projeDurum
    {
        public string projeBaslik { get; set; }
        public string projeAciklama { get; set; }
        public DateTime? iletilenTarih { get; set; }
        public DateTime? gorulenTarih { get;set; }
        public string durumAd { get; set; }
        public string yorum {  get; set; }

    }
    public class OgrenciController : Controller
    {
        ptsEntities entity = new ptsEntities();
        // GET: Calisan
        public ActionResult Index()
        {
            int yetkiTurID = Convert.ToInt32(Session["OgrenciYetkiTuru"]);

            if(yetkiTurID == 2)
            {
                int ogrencıID = Convert.ToInt32(Session["OgrenciID"]);
                var isler = (from i in entity.ProjeIslem where i.ogrID == ogrencıID && i.projeOkuma == false 
                             orderby i.verilenTarih descending select i).ToList();
                ViewBag.isler = isler;
                return View();
            }

            else
            {
                 return RedirectToAction("Index","Login");
            }
        }
        [HttpPost]
        public ActionResult Index(FormCollection f)
        {
            int projeId = Convert.ToInt32(f["projeID"]);
            int ogrenciID = Convert.ToInt32(Session["OgrenciID"]);

            var tekIslem = (from p in entity.ProjeIslem where p.işlemID == projeId select p).FirstOrDefault();
            tekIslem.projeOkuma = true;
            tekIslem.durumID = 2;
            entity.SaveChanges();
            return RedirectToAction("Takip", "Ogrenci");
        }
        public ActionResult Yap() 
        {
            int yetkiTurID = Convert.ToInt32(Session["OgrenciYetkiTuru"]);

            if (yetkiTurID == 2)
            {
                int ogrencıID = Convert.ToInt32(Session["OgrenciID"]);
                var isler = (from i in entity.ProjeIslem where i.ogrID ==ogrencıID && i.durumID==2 select i ).ToList()
                    .OrderByDescending(i=> i.verilenTarih);
                ViewBag.isler = isler;
                return View();
            }

            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ActionResult Yap(int projeID, string projeYorum)
        {
            var tekProje = (from i in entity.ProjeIslem where i.alinanProjeID == projeID select i).FirstOrDefault();

            if (projeYorum == "") projeYorum = "Alındı";
            tekProje.gorulenTarih = DateTime.Now;
            tekProje.durumID = 3;
            tekProje.projeYorum = projeYorum;
            entity.SaveChanges();

            return RedirectToAction("Index", "Ogrenci");

        }

        public ActionResult Takip()
        {
            int yetkiTurID = Convert.ToInt32(Session["OgrenciYetkiTuru"]);

            if (yetkiTurID == 2)
            {
                int ogrencıID = Convert.ToInt32(Session["OgrenciID"]);
                var isler = (from i in entity.ProjeIslem join d in entity.Durum on i.durumID equals d.durumID where 
                             i.ogrID == ogrencıID select i).ToList().OrderByDescending(i=>i.verilenTarih);

                ProjeDurumViewModel model = new ProjeDurumViewModel();
                List<projeDurum> list = new List<projeDurum>();
                foreach (var item in isler)
                {
                    projeDurum projeDurum = new projeDurum();
                    projeDurum.projeBaslik = item.projeAdi;
                    projeDurum.projeAciklama = item.aciklama;
                    projeDurum.iletilenTarih = item.verilenTarih;
                    projeDurum.gorulenTarih = item.gorulenTarih;
                    projeDurum.durumAd = item.Durum.durumAd;
                    projeDurum.yorum = item.projeYorum;

                    list.Add(projeDurum);
                }
                model.projeDurumlar = list;
                return View(model);
            }

            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        public ActionResult Rapor()
        {
            int yetkiTurID = Convert.ToInt32(Session["OgrenciYetkiTuru"]);
            int ogrenciID = Convert.ToInt32(Session["OgrenciID"]);
            if (yetkiTurID == 2)
            {
               var projeListesi = (from p in entity.DonemlikAlinanProje 
                                join pr in entity.Projeler on p.ProjeID equals pr.ProjeID
                                where p.ogrID == ogrenciID select new ProjelerViewModel
                                {
                                    ProjeID = pr.ProjeID,
                                    ProjeAd = pr.ProjeAd
                                }).ToList();
                ViewBag.projeListesi = projeListesi;
                return View();
            }
            else return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public ActionResult Rapor(HttpPostedFileBase raporDosyasi, int proje)
        {
            int ogrenciID = Convert.ToInt32(Session["OgrenciID"]);
            var ogrElmanID = (from donemPrj in entity.DonemlikAlinanProje
                              join ogrElmId in entity.OgretimElemanlari on donemPrj.ogrElmID equals ogrElmId.SicilNo 
                              join ogrenci in entity.Ogrenciler on donemPrj.ogrID equals ogrenci.ogrID
                            where donemPrj.ProjeID == proje && ogrenci.ogrID == ogrenciID select ogrElmId.SicilNo).FirstOrDefault();
            var alinanProje = entity.DonemlikAlinanProje
                            .FirstOrDefault(p => p.ogrID == ogrenciID && p.ProjeID == proje);

            if (alinanProje == null)
            {
                TempData["mesaj"] = "Proje seçimi yapınız.";
                return RedirectToAction("Rapor", "Ogrenci");
            }

            if (raporDosyasi != null && raporDosyasi.ContentLength > 0)
            {
                string dosyaAdi = Path.GetFileName(raporDosyasi.FileName);
                string uzanti = Path.GetExtension(dosyaAdi);

                string yeniDosyaAdi = $"rapor_{ogrenciID}_{DateTime.Now.Ticks}{uzanti}";

                string kayitYolu = Server.MapPath("~/Uploads/Projeler/");
                if (!Directory.Exists(kayitYolu))
                {
                    Directory.CreateDirectory(kayitYolu);
                }

                string tamYol = Path.Combine(kayitYolu, yeniDosyaAdi);

                raporDosyasi.SaveAs(tamYol);

                OgrenciRaporlari yeniRapor = new OgrenciRaporlari
                {
                    OgrID = ogrenciID,
                    ProjeID = proje,
                    DosyaAdi = dosyaAdi,
                    DosyaYolu = "/Uploads/Raporlar/" + yeniDosyaAdi,
                    YuklenmeTarihi = DateTime.Now,
                    ogrElmID = ogrElmanID
                };

                entity.OgrenciRaporlari.Add(yeniRapor);
                entity.SaveChanges();

                TempData["mesaj"] = "Rapor başarıyla yüklendi.";
            }
            else
            {
                TempData["mesaj"] = "Lütfen bir dosya seçin.";
            }

            return RedirectToAction("Rapor","Ogrenci");
        }

        public ActionResult Toplanti()
        {
            int yetkiTurID = Convert.ToInt32(Session["OgrenciYetkiTuru"]);
            int ogrenciID = Convert.ToInt32(Session["OgrenciID"]);

            if (yetkiTurID == 2)
            {
                var toplantilar = (from t in entity.Toplanti where t.tümOgrenciler == true || t.ogrID == ogrenciID select
                                   new ToplantilarViewModel
                                   {
                                       tarih = t.toplantiTarihi,
                                       id = t.ToplantiID,
                                       link = t.toplantiLinki
                                   }).ToList();

                if (toplantilar.Count > 0)
                {
                    ViewBag.toplantilar = toplantilar;

                }
                else
                {
                    TempData["mesaj"] = "İleri tarihli planlanmış bir toplantı bulunmamaktadır";
                }

                    return View();
            }
            else  return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public ActionResult Toplanti(FormCollection f)
        {
            int toplantiId = Convert.ToInt32(f["toplantiSelect"]);

            var toplantıLinki = (from l in entity.Toplanti where l.ToplantiID == toplantiId select l.toplantiLinki).FirstOrDefault();
            try
            {
                return Redirect(toplantıLinki);
            }
            catch (Exception)
            {
                return RedirectToAction("Index", "Login");
            }
        }
        
        public ActionResult ProjeGonder()
        {
            int yetkiTurID = Convert.ToInt32(Session["OgrenciYetkiTuru"]);
            int ogrenciID = Convert.ToInt32(Session["OgrenciID"]);
            if (yetkiTurID == 2)
            {
                var projeListesi = (from p in entity.DonemlikAlinanProje
                                    join pA in entity.Projeler on p.ProjeID equals pA.ProjeID
                                    where p.ogrID == ogrenciID select new ProjelerViewModel
                                    {
                                        ProjeID = pA.ProjeID,
                                        ProjeAd = pA.ProjeAd
                                    }).ToList();
                ViewBag.projeListesi = projeListesi;
                return View();
            }

            else return RedirectToAction("Index", "Login");
        }
        [HttpPost]
        public ActionResult ProjeGonder(HttpPostedFileBase projeDosyasi, int proje,FormCollection f)
        {
            int ogrenciID = Convert.ToInt32(Session["OgrenciID"]);
            var ogrElmanID = (from donemPrj in entity.DonemlikAlinanProje
                              join ogrElmId in entity.OgretimElemanlari on donemPrj.ogrElmID equals ogrElmId.SicilNo
                              join ogrenci in entity.Ogrenciler on donemPrj.ogrID equals ogrenci.ogrID
                              where donemPrj.ProjeID == proje && ogrenci.ogrID == ogrenciID
                              select ogrElmId.SicilNo).FirstOrDefault();
            var alinanProje = entity.DonemlikAlinanProje
                            .FirstOrDefault(p => p.ogrID == ogrenciID && p.ProjeID == proje);

            if (alinanProje == null)
            {
                TempData["mesaj"] = "Proje seçimi yapınız.";
                return RedirectToAction("Rapor", "Ogrenci");
            }

            if (projeDosyasi != null && projeDosyasi.ContentLength > 0)
            {
                string dosyaAdi = Path.GetFileName(projeDosyasi.FileName);
                string uzanti = Path.GetExtension(dosyaAdi);

                string yeniDosyaAdi = $"rapor_{ogrenciID}_{DateTime.Now.Ticks}{uzanti}";

                string kayitYolu = Server.MapPath("~/Uploads/Projeler/");
                if (!Directory.Exists(kayitYolu))
                {
                    Directory.CreateDirectory(kayitYolu);
                }

                string tamYol = Path.Combine(kayitYolu, yeniDosyaAdi);

                projeDosyasi.SaveAs(tamYol);


                OgrenciProjeGonderim yeniRapor = new OgrenciProjeGonderim
                {
                    ogrID = ogrenciID,
                    projeID = proje,
                    DosyaAdi = dosyaAdi,
                    DosyaYolu = "/Uploads/Raporlar/" + yeniDosyaAdi,
                    yuklenmeTarihi = DateTime.Now,
                    ogrElmID = ogrElmanID,
                    projeAdi = f["ProjeAdi"]
                };

                entity.OgrenciProjeGonderim.Add(yeniRapor);
                entity.SaveChanges();

                TempData["mesaj"] = "Rapor başarıyla yüklendi.";
            }
            else
            {
                TempData["mesaj"] = "Lütfen bir dosya seçin.";
            }

            return RedirectToAction("ProjeGonder", "Ogrenci");
        }
        
    }
}