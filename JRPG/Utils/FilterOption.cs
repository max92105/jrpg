namespace JRPG.Utils
{
    public class FilterOption<T>
    {
        public string IconFile { get; set; }
        public T FilterValue { get; set; }
        public string Label => FilterValue?.ToString() ?? "All";
    }
}
