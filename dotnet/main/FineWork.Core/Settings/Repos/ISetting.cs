using System;

namespace FineWork.Settings.Repos
{
    public interface ISetting
    {
        String Id { get; }

        String Value { get; set; }
    }
}