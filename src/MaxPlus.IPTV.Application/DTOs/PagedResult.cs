namespace MaxPlus.IPTV.Application.DTOs;

public class PagedResult<T>
{
    public IEnumerable<T> Data       { get; set; } = [];
    public int            Total      { get; set; }
    public int            Page       { get; set; }
    public int            PageSize   { get; set; }
    public int            TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)Total / PageSize) : 0;
    public bool           HasNext    => Page < TotalPages;
    public bool           HasPrev    => Page > 1;
}
