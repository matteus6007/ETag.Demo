namespace ETag.Demo.Api.Models
{
    public class ETag
    {
        public ETagType ETagType { get; }
        public string Value { get; }

        public ETag(ETagType eTagType, string value)
        {
            ETagType = eTagType;
            Value = value;
        }

        public override string ToString()
        {
            switch (ETagType)
            {
                case ETagType.Strong:
                    return $"\"{Value}\"";

                case ETagType.Weak:
                    return $"W\"{Value}\"";

                default:
                    return $"\"{Value}\"";
            }
        }
    }
}
