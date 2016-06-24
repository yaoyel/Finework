namespace FineWork.Avatar
{
    /// <summary> 定义了一组常见的头像尺寸. </summary>
    public enum AvatarSizes
    {
        /// <summary> 通常用于与其他头像聚合成九宫格. </summary>
        ExtraSmall = 18, 

        /// <summary> 通常用于聊天页. </summary>
        Small = 46,

        /// <summary> 通常用于列表页. </summary>
        Medium = 64,   

        /// <summary> 通常用于个人/组织详细信息页. </summary>
        Large = 96,  

        /// <summary> 通常用于登录页. </summary>
        ExtraLarge = 132,

        /// <summary> The original image size. </summary>
        Full = 640 
    }
}