using Walgelijk;

namespace Orbix
{
    public struct NameGenerator
    {
        public static string GenerateName()
        {
            string[] prefixes = new[] {
            "Fl",
            "Fn",
            "Fi",
            "Fs",
            "Fo",
            "F",
            "Dl",
            "Dn",
            "Di",
            "Ds",
            "Do",
            "D",
            "Wl",
            "Wn",
            "Wi",
            "Ws",
            "Wo",
            "W",
            "Bl",
            "Bn",
            "Bi",
            "Bs",
            "Bo",
            "B",
            "Sl",
            "Sn",
            "Si",
            "Ss",
            "So",
            "S",
            "L",
            "Li",
            "Lo",
            "Sch",
            "Ml",
            "Mi",
            "Me",
            "Ka",
            "Mo",
            "M",
            "E",
            "P",
            "Pl",
            "Pe",
            "Po",
            "Pi",
            "Pa"};

            string[] basis = new[] {
            "ur",
            "uro",
            "oro",
            "ibo",
            "ub",
            "on",
            "stie",
            "ip",
            "ka",
            "opo",
            "oba",
            "obbi",
            "epa",
            "imp",
            "abe",
            "wip",
            "iko",
            "ulo",
            "ale",
            "olla",
            "mik",
            "ekk",
            "ill",
            "wot",
            "e" };

            string[] suffixes = new[] {
            "bos",
            "los",
            "les",
            "res",
            "mos",
            "sos",
            "er",
            "z",
            "kaka",
            "erkle",
            "erkel",
            "o",
            "a",
            "e",
            "mlo",
            "ckle",
            "kel",
            "derckle",
            "derkel",
            "inkel",
            "e",
            "o",
            "sh",
            "se",
            "es",
            "us",
            "is",
            "b",
            "d" };

            string chosenPrefix = Utilities.PickRandom(prefixes);
            string chosenBasis = Utilities.PickRandom(basis);
            string chosenSuffix = Utilities.PickRandom(suffixes);
            return chosenPrefix + chosenBasis + chosenSuffix;
        }
    }
}
