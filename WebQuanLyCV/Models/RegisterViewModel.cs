using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebQuanLyCV.Models
{
    public class RegisterViewModel
    {
        public int MaTTTK { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public Nullable<System.DateTime> NgaySinh { get; set; }
        public string TrinhDoHocVan { get; set; }
        public string GioiThieuBanThan { get; set; }
        public Nullable<int> MaLoaiTK { get; set; }
        public Nullable<int> MaCT { get; set; }
        public string MatKhau { get; set; }
        public string CauHoiAN { get; set; }
        public string AnhDD { get; set; }
        public string SDTCN { get; set; }
        public string TenCT { get; set; }
        public string DiaChi { get; set; }
        public string SDT { get; set; }
        public string EmailCT { get; set; }
        public string Fax { get; set; }
    }
}