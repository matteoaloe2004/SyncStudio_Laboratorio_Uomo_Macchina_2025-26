using System.Collections.Generic;
using Template.Services.Shared;

namespace Template.Web.Features.Home
{
    public class HomeViewModel
    {
        public List<CorsoDTO> Corsi { get; set; }
        public List<StanzaStudioDTO> Stanze { get; set; }
        public string NickName { get; set; }
        public int TotalStudentiOnline { get; set; }
        public StanzaStudioDTO StanzaConsigliata { get; set; }
        public double OreStudioSettimana { get; set; }
        public int GiorniDiFila { get; set; }
        public double OreStudioOggi { get; set; }
        public double[] OreStudioSettimanali { get; set; } = new double[7];
    }
}
