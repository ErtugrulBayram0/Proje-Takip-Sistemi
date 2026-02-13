using ProjeTakipSistemi.Models;
using ProjeTakipSistemi.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ProjeTakipSistemi.Services;


namespace ProjeTakipSistemi.Controllers
{
    public class YoneticiController : Controller
    {
        ptsEntities entity = new ptsEntities();
        public ActionResult Index()
        {
            int yetkiTurId = Convert.ToInt32(Session["OgretimElemaniYetkiTuru"]);
            if(yetkiTurId == 1)
            {
                return View();
            }

            else  return RedirectToAction("Index", "Login");
        }

        public ActionResult Ata()
        {
            int yetkiTurId = Convert.ToInt32(Session["OgretimElemaniYetkiTuru"]);
            if (yetkiTurId == 1)
            {
                int yoneticiID = Convert.ToInt32(Session["OgretimELemanID"]);
                var ogrenciler = (from o in entity.DonemlikAlinanProje
                                  join h in entity.OgretimElemanlari on o.ogrElmID equals h.SicilNo
                                  join ogr in entity.Ogrenciler on o.ogrID equals ogr.ogrID
                                  where o.ogrElmID == yoneticiID
                                  select new OgrenciViewModel
                                  {
                                      ogrID = ogr.ogrID,
                                      ogrAd = ogr.ogrAd,
                                      ogrSoyad = ogr.ogrSoyad
                                  }).ToList();

                ViewBag.ogrenciler = ogrenciler;

                var projeListesi = (from p in entity.Projeler
                                    select new ProjelerViewModel
                                    {
                                        ProjeID = p.ProjeID,
                                        ProjeAd = p.ProjeAd,

                                    }).ToList();
                ViewBag.projeListesi = projeListesi;
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }





            
            
        }
        [HttpPost]
        public ActionResult Ata(FormCollection formCollection)
        {
            string projeBaslik = formCollection["projeBaslik"];
            string projeAciklama = formCollection["projeAciklama"];
            int secilenOgrId = Convert.ToInt32(formCollection["selectOgr"]);
           
            ProjeIslem yeniProje = new ProjeIslem();

            int projeId = Convert.ToInt32(projeBaslik);
            yeniProje.projeAdi = (from p in entity.Projeler where p.ProjeID == projeId select p.ProjeAd).FirstOrDefault();
            yeniProje.aciklama = projeAciklama;
            yeniProje.ogrID = secilenOgrId;
            yeniProje.verilenTarih = DateTime.Now;
            yeniProje.durumID = 1;
            yeniProje.projeOkuma = false;
            yeniProje.alinanProjeID = (from a in entity.DonemlikAlinanProje where a.ProjeID == projeId && a.ogrID == secilenOgrId select a.AlinanProjeID).FirstOrDefault(); 

            entity.ProjeIslem.Add(yeniProje);
            entity.SaveChanges();

            
            return RedirectToAction("Takip","Yonetici");
        }

        [HttpGet]
        public ActionResult Takip()
        {
            int yetkiTurId = Convert.ToInt32(Session["OgretimElemaniYetkiTuru"]);
            if (yetkiTurId == 1)
            {
                int yoneticiID = Convert.ToInt32(Session["OgretimELemanID"]);

                var projeListesi = (from p in entity.Projeler
                                    select new ProjelerViewModel
                                    {
                                        ProjeID = p.ProjeID,
                                        ProjeAd = p.ProjeAd,
                                    }).ToList();
                ViewBag.projeListesi = projeListesi;

                ViewBag.ogrenciler = new List<OgrenciViewModel>();

                return View();
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ActionResult Takip(int? proje, int? selectOgr)
        {
            int yetkiTurId = Convert.ToInt32(Session["OgretimElemaniYetkiTuru"]);
            if (yetkiTurId == 1)
            {
                if (proje.HasValue && !selectOgr.HasValue)
                {
                    int ogrElmID = Convert.ToInt32(Session["OgretimELemanID"]);

                    var projeListesi = (from p in entity.Projeler
                                        select new ProjelerViewModel
                                        {
                                            ProjeID = p.ProjeID,
                                            ProjeAd = p.ProjeAd,
                                        }).ToList();
                    ViewBag.projeListesi = projeListesi;

                    int secilenProjeID = proje ?? 0;

                    var ogrenciler = (from o in entity.DonemlikAlinanProje
                                      join yon in entity.OgretimElemanlari on o.ogrElmID equals yon.SicilNo
                                      join ogr in entity.Ogrenciler on o.ogrID equals ogr.ogrID
                                      where o.ogrElmID == ogrElmID && o.ProjeID == secilenProjeID
                                      select new OgrenciViewModel
                                      {
                                          ogrID = ogr.ogrID,
                                          ogrAd = ogr.ogrAd,
                                          ogrSoyad = ogr.ogrSoyad
                                      }).ToList();

                    ViewBag.ogrenciler = ogrenciler;
                    if (ogrenciler.Count == 0)
                    {
                        ViewBag.mesaj = "Seçilen projeye ait öğrenci bulunamadı.";
                    }


                    return View();
                }

                else if (selectOgr.HasValue)
                {
                    var secilenOgrenci = (from o in entity.Ogrenciler where o.ogrID == selectOgr select o).FirstOrDefault();
                    TempData["secilen"] = secilenOgrenci;
                    return RedirectToAction("Listele", "Yonetici");
                } 
            }
            return View();
        }

        [HttpGet]
        public ActionResult Listele()
        {
            int yetkiTurId = Convert.ToInt32(Session["OgretimElemaniYetkiTuru"]);
            int ogrElmID = Convert.ToInt32(Session["OgretimELemanID"]);

            if (yetkiTurId == 1)
            {
                
                Ogrenciler secilenOgrenci = (Ogrenciler)TempData["secilen"];

                try
                {
                    var isler = (from i in entity.ProjeIslem 
                                 join d in entity.DonemlikAlinanProje on i.alinanProjeID equals d.AlinanProjeID
                                 where i.ogrID == secilenOgrenci.ogrID select i).FirstOrDefault();

                    var durum = (from dr in entity.Durum where dr.durumID ==isler.durumID select dr.durumAd).FirstOrDefault(); 
                    ViewBag.isler = isler;
                    ViewBag.ogrenci = secilenOgrenci;
                    ViewBag.durum = durum;
                    return View();

                }
                catch (Exception)
                {
                    return RedirectToAction("Takip", "Yonetici");
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        
        }

        [HttpPost]

        public ActionResult Listele(FormCollection f)
        {
            return RedirectToAction("Takip", "Yonetici");
        }

        public ActionResult Raporlar(int? ogrenciID)
        {
            int yetkiTurId = Convert.ToInt32(Session["OgretimElemaniYetkiTuru"]);
            if (yetkiTurId == 1)
            {
                int ogrElmID = Convert.ToInt32(Session["OgretimELemanID"]);
                var ogrenciler = (from donem_proje in entity.DonemlikAlinanProje
                                  join ogr in entity.Ogrenciler on donem_proje.ogrID
                                  equals ogr.ogrID
                                  where donem_proje.ogrElmID == ogrElmID
                                  select new
                                  {
                                      ogr.ogrID,
                                      AdSoyad = ogr.ogrAd + " " + ogr.ogrSoyad
                                  }).ToList();

                ViewBag.OgrenciListesi = new SelectList(ogrenciler, "ogrID", "AdSoyad");


                var raporlar = (from r in entity.OgrenciRaporlari
                                join o in entity.Ogrenciler on r.OgrID equals o.ogrID
                                join p in entity.Projeler on r.ProjeID equals p.ProjeID
                                join e in entity.OgretimElemanlari on r.ogrElmID equals e.SicilNo
                                where (!ogrenciID.HasValue || r.OgrID == ogrenciID.Value) && r.ogrElmID == ogrElmID
                                select new RaporBilgileriViewModel
                                {
                                    RaporID = r.RaporID,
                                    DosyaAdi = r.DosyaAdi,
                                    DosyaYolu = r.DosyaYolu,
                                    YuklenenTarih = r.YuklenmeTarihi,
                                    OgrenciAdSoyad = o.ogrAd + " " + o.ogrSoyad,
                                    ProjeAd = p.ProjeAd
                                }).ToList();
                if (ogrenciID.HasValue)
                {
                    if (raporlar.Count != 0)
                    {
                        ViewBag.Raporlar = raporlar;
                    }
                    else
                    {
                        TempData["Mesaj"] = "Öğrenciye ait rapor bulunamadı";
                    }

                }
                ViewBag.SeciliOgrenciID = ogrenciID;
                return View();
            }

            else return RedirectToAction("Index", "Login");

        }




        public ActionResult Toplanti()
        {
            int yetkiTurId = Convert.ToInt32(Session["OgretimElemaniYetkiTuru"]);
            int ogrElmID = Convert.ToInt32(Session["OgretimELemanID"]);

            if (yetkiTurId == 1)
            {
                var toplantiOgrenci = (from t in entity.DonemlikAlinanProje
                                       join
                                       o in entity.Ogrenciler on t.ogrID equals o.ogrID
                                       join
                                       h in entity.OgretimElemanlari on t.ogrElmID equals h.SicilNo
                                       where t.ogrElmID == ogrElmID
                                       select new OgrenciViewModel
                                       {
                                           ogrID = o.ogrID,
                                           ogrAd = o.ogrAd,
                                           ogrSoyad = o.ogrSoyad
                                       }).ToList();
                ViewBag.toplantiOgrenci = toplantiOgrenci;
                return View();
            }
            else return RedirectToAction("Index", "Login");

        }



        [HttpPost]
        public async Task<ActionResult> Toplanti(FormCollection formCollection)
        {
            int ogrElmID = Convert.ToInt32(Session["OgretimELemanID"]);
            int ogrID = Convert.ToInt32(formCollection["toplantiSelect"]);
            DateTime tarih = Convert.ToDateTime(formCollection["tarihAyarla"]);
            string saat = formCollection["saatAyarla"];
            DateTime toplantiTarihi = tarih.Date
                                    .Add(TimeSpan.Parse(saat));

            string aciklama = formCollection["toplantiAciklama"];


            var zoomService = new ZoomService("wQE1ahBQoy857woNyShOg", "yergk1MA8u0LK1yljC6HGXAAs0YgZDI1", "YR-3g34sR_mtFG4zCdNwvw");
            var meeting = await zoomService.CreateMeetingAsync("Proje Toplantısı " ,  toplantiTarihi, 40);


            Toplanti yeniToplanti = new Toplanti()
            {
                ogrElmID = ogrElmID,
                toplantiTarihi = toplantiTarihi,
                toplantiAciklama = aciklama,
                toplantiLinki = (string)meeting.join_url,
            };
            if (formCollection["checkDefault"]== "on")
            {
                yeniToplanti.tümOgrenciler = true;
            }
            else
            {
                yeniToplanti.ogrID = ogrID;
            }
            entity.Toplanti.Add(yeniToplanti);
            entity.SaveChanges();

            ViewBag.Mesaj = "Toplantı başarıyla oluşturuldu. Link: " + meeting.join_url;

            // Öğrenci listesini yeniden yükle
            var toplantiOgrenci = (from t in entity.DonemlikAlinanProje
                                   join o in entity.Ogrenciler on t.ogrID equals o.ogrID
                                   join h in entity.OgretimElemanlari on t.ogrElmID equals h.SicilNo
                                   where t.ogrElmID == ogrElmID
                                   select new OgrenciViewModel
                                   {
                                       ogrID = o.ogrID,
                                       ogrAd = o.ogrAd,
                                       ogrSoyad = o.ogrSoyad
                                   }).ToList();

            ViewBag.toplantiOgrenci = toplantiOgrenci;

            return View();
        }

        public ActionResult ToplantiKatil()
        {
            int ogrElmID = Convert.ToInt32(Session["OgretimELemanID"]);
            int yetkiTurId = Convert.ToInt32(Session["OgretimElemaniYetkiTuru"]);
            if (yetkiTurId == 1)
            {
                DateTime gunceltarih = DateTime.Now;
                var toplantilar = (from t in entity.Toplanti where t.ogrElmID == ogrElmID && t.toplantiTarihi > gunceltarih 
                                   select new ToplantilarViewModel
                                   {
                                     link = t.toplantiLinki,
                                     tarih = t.toplantiTarihi,
                                     id = t.ToplantiID
                                   }).ToList();
                if (toplantilar.Count > 0)
                {
                    ViewBag.toplantilar = toplantilar;
                }
                else TempData["mesaj"] = "İleri tarihli planlanmış bir toplantınız bulunmamaktadır.";
                    return View();
            }
            else return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public ActionResult ToplantiKatil(FormCollection form)
        {
            int toplantiId = Convert.ToInt32(form["toplantiSelect"]);

            var toplanti = (from t in entity.Toplanti where t.ToplantiID == toplantiId select t.toplantiLinki).FirstOrDefault();
            if (toplanti != null)
            {
                return Redirect(toplanti);
            }
            else
            {
                TempData["mesaj"] = "Toplantı linki bulunamadı";
                return RedirectToAction("Toplanti", "Yonetici");
            }
        }

    }
}