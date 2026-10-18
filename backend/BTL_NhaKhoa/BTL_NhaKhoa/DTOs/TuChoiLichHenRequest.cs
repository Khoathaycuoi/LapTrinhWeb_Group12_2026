using System.ComponentModel.DataAnnotations;

namespace BTL_NhaKhoa.DTOs;

public sealed class TuChoiLichHenRequest
{
    [Required(ErrorMessage = "Vui lòng nhập lý do từ chối.")]
    [StringLength(500)]
    public string LyDo { get; set; } = string.Empty;
}
