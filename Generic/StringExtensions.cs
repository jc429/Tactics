
public static class StringExtensions
{
	/* wraps quotes around a string (for sql commands) */
    public static string Enquote (string input){
		return "\"" + input + "\"";
	}
}
