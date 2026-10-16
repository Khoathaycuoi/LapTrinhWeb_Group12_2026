namespace BTL_NhaKhoa.DTOs;

public sealed class ChamCongHomNayResponse
{
    public DateTime GioServer { get; set; }
    public ChamCongResponse? ChamCong { get; set; }
    public bool CoTheCheckInSang { get; set; }
    public bool CoTheCheckOutSang { get; set; }
    public bool CoTheCheckInChieu { get; set; }
    public bool CoTheCheckOutChieu { get; set; }
}
