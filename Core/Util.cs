using System;

public static class Util
{
    const string ALPHANUM = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public static string GenerateRandomString(int length)
    {
        if (length < 0)
        {
            throw new ArgumentException("Length cannot be negative");
        }

        Random r = new Random();
        string s = "";
        for (int i = 0; i < length; i++)
        {
            s += ALPHANUM[r.Next(ALPHANUM.Length)];
        }
        return s;
    }
}
