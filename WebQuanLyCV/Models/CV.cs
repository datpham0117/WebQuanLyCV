//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebQuanLyCV.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class CV
    {
        public int MaCV { get; set; }
        public Nullable<int> MaTTTK { get; set; }
        public Nullable<int> MaChV { get; set; }
        public Nullable<int> MaCT { get; set; }
        public string TieuDeCV { get; set; }
        public Nullable<bool> DaXem { get; set; }
        public string KhaNang { get; set; }
    
        public virtual ChucVu ChucVu { get; set; }
        public virtual CongTy CongTy { get; set; }
        public virtual ThongtinTaiKhoan ThongtinTaiKhoan { get; set; }
    }
}
