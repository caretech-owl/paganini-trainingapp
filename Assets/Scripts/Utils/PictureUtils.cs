using System;


public static class PictureUtils 
{

    public static byte[] ConvertBase64ToByteArray(string base64){
        if (!string.IsNullOrWhiteSpace(base64)){
            return Convert.FromBase64String(base64);
        }
        return null;
    }

    public static string ConvertByteArrayToBase64(byte[] byteArray){
        return byteArray != null ? Convert.ToBase64String(byteArray) : null;
    }
}
