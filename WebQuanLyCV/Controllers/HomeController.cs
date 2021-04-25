using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebQuanLyCV.Models;
using CaptchaMvc.HtmlHelpers;
using CaptchaMvc;
using System.IO;
using System.Net;
using PagedList;

namespace WebQuanLyCV.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        WebQuanLyCVEntities1 db = new WebQuanLyCVEntities1();
        #region phần đăng nhập
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(FormCollection f, string email, string pass)
        {
            email = f.Get("txtEmail").ToString();
            pass = f.Get("txtPass").ToString();
            ThongtinTaiKhoan Tttkh = db.ThongtinTaiKhoans.SingleOrDefault(n => n.Email == email && n.MatKhau == pass);
            if (Tttkh != null)
            {
                Session["TaiKhoan"] = Tttkh;
                return RedirectToAction("ProductCV", "Home");
            }
            return View(Tttkh);
        }
        
        public ActionResult Logout()
        {
            Session["TaiKhoan"] = null;
            return RedirectToAction("Login", "Home");
        }
        #endregion

        #region Phần hiển thị CV
        public ActionResult ProductCV(int? page)
        {
            //ViewBag.listCV = db.CVs.ToList();
            ViewBag.listChV = db.ChucVus.ToList();
            ViewBag.listCT = db.CongTies.ToList();
            //Hiển thị những công việc được quan tâm
            //Thực hiện chức năng phân trang
            var listCV = db.CVs.ToList();
            int PageSize = 12;// 12 thành phần 1 trang
            int PageNumber = (page ?? 1);// số lượng trang không tăng thì sẽ mặc định bằng 1
           
            return View(listCV.ToPagedList(PageNumber,PageSize));
        }
        #endregion

        #region phần đăng ký
        //Lấy ra các TextBox trong Regiter.cshtml có cùng tên với tên của các cột trong bảng ThongtinTaiKhoan
        //Phần đăng kí tài khoản mới
        [HttpGet]
        public ActionResult Register()
        {
            ViewBag.MaLoaiTK = new SelectList(db.LoaiTaiKhoans.OrderBy(n => n.MaLoaiTK), "MaLoaiTK", "TenLoaiTK");
            ViewBag.CauHoi = new SelectList(LoadCauHoi());
            return View();
        }
        
        [HttpPost]
        //Trong HttpPostedFileBase luôn đặt tên parameter trùng với CSDL để truyền hình ảnh
        public ActionResult Register(ThongtinTaiKhoan tttk, RegisterViewModel rgm, FormCollection f, HttpPostedFileBase AnhDD, string Email)
        {
            ViewBag.MaLoaiTK = new SelectList(db.LoaiTaiKhoans.OrderBy(n => n.MaLoaiTK), "MaLoaiTK", "TenLoaiTK");
            ViewBag.CauHoi = new SelectList(LoadCauHoi());

            //Add thông tin của 2 bảng cùng 1 lúc cần tạo 1 viewModel làm trung gian giữa 2 bảng
            CongTy ct = new CongTy();

            ct.TenCT = rgm.TenCT;
            ct.DiaChi = rgm.DiaChi;
            ct.SDT = rgm.SDT;
            ct.EmailCT = rgm.EmailCT;
            ct.Fax = rgm.Fax;
            //int MaCT = ct.MaCT;

            tttk.HoTen = rgm.HoTen;
            tttk.Email = rgm.Email;
            tttk.NgaySinh = rgm.NgaySinh;
            tttk.TrinhDoHocVan = rgm.TrinhDoHocVan;
            tttk.GioiThieuBanThan = rgm.GioiThieuBanThan;
            tttk.MaLoaiTK = rgm.MaLoaiTK;
            tttk.AnhDD = rgm.AnhDD;
            tttk.SDTCN = rgm.SDTCN;
            if (tttk.MaLoaiTK == 2)
            {
                int MaCT = ct.MaCT;
                tttk.MaCT = MaCT;
            }
            
            //Kiểm tra hình ảnh có tồn tại trong csdl chưa, nếu chưa thì upload hình ảnh lên 
            if(AnhDD.ContentLength >= 0)
            {
                var fileName = Path.GetFileName(AnhDD.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                if(System.IO.File.Exists(path))
                {
                    ViewBag.Upload = "Hình ảnh đã tồn tại";
                    return View();
                }
                else
                {
                    AnhDD.SaveAs(path);
                    tttk.AnhDD = fileName;
                    //Session["Nameimage"] = AnhDD.FileName;
                    //ViewBag.Nameimage = "";
                }
            }
            //Kiểm tra Captcha hợp lệ
            if (this.IsCaptchaValid("Captcha is not valid"))
            {
                ViewBag.ThongBao = "Thêm thành công";
                //Add thêm thông tin nhập vào từ TextBox vào ThongtinTaiKhoan
                if (db.ThongtinTaiKhoans.Any(n => n.Email == Email))
                {
                    ViewBag.Upload1 = "Địa chỉ Email đã tồn tại";
                    return View();
                }
                else
                {
                    db.ThongtinTaiKhoans.Add(tttk);
                }
                if (tttk.MaLoaiTK == 2)
                {
                    db.CongTies.Add(ct);
                }
                db.SaveChanges();
                return RedirectToAction("Login");
            }
            ViewBag.ThongBao1 = "Sai mã Captcha";
            return View();

        }

        public ActionResult Register1()
        {
            return View();
        }

        //Tạo dữ liệu cho dropdownbox
        public List<string>LoadCauHoi()
        {
            List<string> lstCauHoi = new List<string>();
            lstCauHoi.Add("trò chơi yêu thích?");
            lstCauHoi.Add("bài hát yêu thích?");
            lstCauHoi.Add("Cửa hàng yêu thích?");
            return lstCauHoi;
        }
        #endregion

        #region phần xem chi tiết CV
        [HttpGet]
        public ActionResult XemChiTiet(int? MaCV)
        {
            if (MaCV == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var ListCV = db.CVs.Where(n => n.MaCV == MaCV);
            if (ListCV.Count() == 0)
            {
                return HttpNotFound();
            }
            return View(ListCV);
        }
        [HttpPost]
        public ActionResult XemChiTiet(CV cv)
        {
            db.Entry(cv).State = System.Data.Entity.EntityState.Modified;
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return RedirectToAction("ProductCV");
            }
            return View(cv);
        }
        #endregion

        #region phần chỉnh sửa thông tin tài khoản
        [HttpGet]
        public ActionResult ChinhSuaTK(int? MaTTTK)
        {
            if(MaTTTK == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ThongtinTaiKhoan tttk = db.ThongtinTaiKhoans.SingleOrDefault(n => n.MaTTTK == MaTTTK);
            ViewBag.MaLoaiTK = new SelectList(db.LoaiTaiKhoans.OrderBy(n => n.MaLoaiTK), "MaLoaiTK", "TenLoaiTK", tttk.MaLoaiTK);
            ViewBag.CauHoi = new SelectList(LoadCauHoi());
            
            if(tttk == null)
            {
                return HttpNotFound();
            }
            return View(tttk);
        }
        [HttpPost]
        public ActionResult ChinhSuaTK(ThongtinTaiKhoan tttk, HttpPostedFileBase AnhDD)
        {
            ViewBag.MaLoaiTK = new SelectList(db.LoaiTaiKhoans.OrderBy(n => n.MaLoaiTK), "MaLoaiTK", "TenLoaiTK", tttk.MaLoaiTK);
            ViewBag.CauHoi = new SelectList(LoadCauHoi());
            if (AnhDD.ContentLength > 0)
            {
                var fileName = Path.GetFileName(AnhDD.FileName);
                var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                if (System.IO.File.Exists(path))
                {
                    ViewBag.Upload = "Hình ảnh đã tồn tại";
                    return View();
                }
                else
                {
                    AnhDD.SaveAs(path);
                    tttk.AnhDD = fileName;
                    //Session["Nameimage"] = AnhDD.FileName;
                    //ViewBag.Nameimage = "";
                }
            }
            db.Entry(tttk).State = System.Data.Entity.EntityState.Modified;
            if (ModelState.IsValid)
            {
                db.SaveChanges();
                return RedirectToAction("ProductCV");
            }
            return View(tttk);
        }
        #endregion

        #region phần xóa tài khoản
        [HttpGet]
        public ActionResult XoaTK(int? MaTTTK)
        {
            if (MaTTTK == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ThongtinTaiKhoan tttk = db.ThongtinTaiKhoans.SingleOrDefault(n => n.MaTTTK == MaTTTK);
            ViewBag.MaLoaiTK = new SelectList(db.LoaiTaiKhoans.OrderBy(n => n.MaLoaiTK), "MaLoaiTK", "TenLoaiTK", tttk.MaLoaiTK);
            ViewBag.CauHoi = new SelectList(LoadCauHoi());

            if (tttk == null)
            {
                return HttpNotFound();
            }
            return View(tttk);
        }
        [HttpPost]
        public ActionResult XoaTK(int MaTTTK)
        {
            ThongtinTaiKhoan tttk = db.ThongtinTaiKhoans.SingleOrDefault(n => n.MaTTTK == MaTTTK);
            ViewBag.MaLoaiTK = new SelectList(db.LoaiTaiKhoans.OrderBy(n => n.MaLoaiTK), "MaLoaiTK", "TenLoaiTK", tttk.MaLoaiTK);
            ViewBag.CauHoi = new SelectList(LoadCauHoi());
            if (MaTTTK == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if(tttk == null)
            {
                return HttpNotFound();
            }
            db.ThongtinTaiKhoans.Remove(tttk);
            db.SaveChanges();
            return RedirectToAction("Login");
        }
        #endregion

        #region phần tạo thông tin tuyển dụng
        [HttpGet]
        public ActionResult MakeJob(int MaCT)
        {
            ViewBag.listTTTK = db.ThongtinTaiKhoans.Where(n => n.MaCT == MaCT && n.MaLoaiTK == 2);
            ViewBag.listCT = db.CongTies.Where(n => n.MaCT == MaCT);
            return View();
        }
        [HttpPost]
        public ActionResult MakeJob(ChucVu cv, CongTy ct)
        {
            int MaCT = ct.MaCT;
            cv.MaCT = MaCT;
            ViewBag.listTTTK = db.ThongtinTaiKhoans.Where(n => n.MaCT == MaCT && n.MaLoaiTK == 2);
            ViewBag.listCT = db.CongTies.Where(n => n.MaCT == MaCT);
            db.ChucVus.Add(cv);
            db.SaveChanges();
            return RedirectToAction("ProductCV");
        }
        #endregion

        #region Load danh sách tuyển dụng
        public ActionResult LoadJobsList(int? page)
        {
            //ViewBag.listChV = db.ChucVus.ToList();
            ViewBag.listCT = db.CongTies.ToList();
            var listChV = db.ChucVus.ToList();
            int PageSize = 12;// 12 thành phần 1 trang
            int PageNumber = (page ?? 1);// số lượng trang không tăng thì sẽ mặc định bằng 1
            return View(listChV.ToPagedList(PageNumber, PageSize));
        }
        #endregion

        #region phần xem chi tiết job
        [HttpGet]
        public ActionResult ViewJob(int MaTTTK, int MaChV, int MaCT)
        {
            ViewBag.listTTTK = db.ThongtinTaiKhoans.Where(n => n.MaTTTK == MaTTTK);
            ViewBag.listChV = db.ChucVus.Where(n => n.MaChV == MaChV && n.MaCT == MaCT);
            ViewBag.listCT = db.CongTies.Where(n => n.MaCT == MaCT);
            return View();
        }
        [HttpPost]
        public ActionResult ViewJob(CV cv, int MaTTTK, int MaChV,int MaCT)
        {
            ViewBag.listTTTK = db.ThongtinTaiKhoans.Where(n => n.MaTTTK == MaTTTK);
            ViewBag.listChV = db.ChucVus.Where(n => n.MaChV == MaChV && n.MaCT == MaCT);
            ViewBag.listCT = db.CongTies.Where(n => n.MaCT == MaCT);
            if (db.CVs.Any(n => n.MaTTTK == MaTTTK && n.MaChV == MaChV && n.MaCT == MaCT))
            {
                ViewBag.Upload = "Bạn đã ứng tuyển công việc này!";
                return View();
            }
            else
            {
                db.CVs.Add(cv);
                db.SaveChanges();
            }
            return RedirectToAction("ProductCV");
        }
        #endregion

        #region Chỉnh sửa thông tin đăng tuyển
        [HttpGet]
        public ActionResult ChinhSuaChV(int? MaChV)
        {
            if(MaChV == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ChucVu ChV = db.ChucVus.SingleOrDefault(n => n.MaChV == MaChV);
            if (ChV == null)
            {
                return HttpNotFound();
            }
            return View(ChV);
        }
        [HttpPost]
        public ActionResult ChinhSuaChV(ChucVu ChV)
        {
            db.Entry(ChV).State = System.Data.Entity.EntityState.Modified;
            if(ModelState.IsValid)
            {
                db.SaveChanges();
                return RedirectToAction("LoadJobsList");
            }
            return View(ChV);
        }
        #endregion

        #region Xóa thông tin đăng tuyển
        [HttpGet]
        public ActionResult XoaChV(int? MaChV)
        {
            if (MaChV == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            ChucVu ChV = db.ChucVus.SingleOrDefault(n => n.MaChV == MaChV);
            if (ChV == null)
            {
                return HttpNotFound();
            }
            return View(ChV);
        }
        [HttpPost]
        public ActionResult XoaChV(int MaChV)
        {
            ChucVu ChV = db.ChucVus.SingleOrDefault(n => n.MaChV == MaChV);
            if (MaChV == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (ChV == null)
            {
                return HttpNotFound();
            }
            db.ChucVus.Remove(ChV);
            db.SaveChanges();
            return RedirectToAction("LoadJobsList");
        }
        #endregion

        #region Chỉnh sửa thông tin công ty
        [HttpGet]
        public ActionResult ChinhSuaCT(int? MaCT)
        {
            if (MaCT == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            CongTy CT = db.CongTies.SingleOrDefault(n => n.MaCT == MaCT);
            if (CT == null)
            {
                return HttpNotFound();
            }
            return View(CT);
        }
        [HttpPost]
        public ActionResult ChinhSuaCT(CongTy CT)
        {
            db.Entry(CT).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("ProductCV");
        }
        #endregion
    }
}