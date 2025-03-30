namespace StringConv
{
    public sealed class CustomEncoding
    {
        public int CodePage { get; set; }
        public string Name { get; set; }

        public CustomEncoding(int codePage, string name)
        {
            CodePage = codePage;
            Name = name;
        }
    }
}
