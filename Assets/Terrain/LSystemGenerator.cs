using System.Collection.Generics;
using System.Text;

public class LSystemGenerator
{
    // Attributes.
    private string axiom;                       // Initial production character.
    private Dictionary<char, string> rules;     // Actually existing rules.
    private int iterations;                     // Number of iterations to run the LSystemGenerator for.

    public LSystemGenerator(string axiom, Dictionary<char, string> rules, int iterations)
    {
        this.axiom = axiom;
        this.rules = rules;
        this.iterations = iterations;
    }

    public string GenerateString()
    {
        StringBuilder actualStr = new StringBuilder(this.axiom);
        foreach(int i; i < iterations; ++i)
        {
            StringBuilder newStr = new StringBuilder("");
            foreach (char actualSymbol in actualStr)
            {
                if(this.rules.TryGetValue(actualSymbol, out string rule))
                {
                    newStr.Append(rule);
                }
                else
                {
                    newStr.Append(actualSymbol);
                }
            }
            actualStr.Clear();
            actualStr.Append(newStr);
        }

        return actualStr.ToString();
    }

}