namespace Spine.Common.Data.Interfaces
{
    public interface IPagedRequest
    {
        int Page { get; set; }
        int PageLength { get; set; }
    }
}
