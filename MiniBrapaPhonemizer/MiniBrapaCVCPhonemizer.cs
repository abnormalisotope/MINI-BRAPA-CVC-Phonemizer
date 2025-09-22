using OpenUtau.Api;
using OpenUtau.Plugin.Builtin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MiniBrapaPhonemizer
{
    [Phonemizer("MINI BRAPA CVC Phonemizer", "BRAPA CVC", "Team BRAPA + ly", language: "PT")]
    public class MiniBrapaCVCPhonemizer : SyllableBasedPhonemizer
    {
        /// <summary>
        /// Brazilian Portuguese CVC Phonemizer by Team BRAPA
        /// Doesn't use G2P Dictionary
        /// Support for BRAPA connotation
        /// Alias: - C, - V, C -, V -, -C V, V C-, C V, V C, V, V V
        /// </summary>
        private readonly string[] vowels = "a,oa,ah,ahn,ax,an,e,en,eh,ehn,ae,aen,i,in,i0,o,on,oh,ohn,u,un,u0".Split(",");
        private readonly string[] burstConsonants = "b,ch,d,dj,g,k,p,t".Split(",");
        private string[] shortConsonants = "r".Split(",");
        private string[] longConsonants = "s,sh".Split(",");
        protected override string[] GetVowels() => vowels;
        protected override List<string> ProcessSyllable(Syllable syllable)
        {
            string prevV = syllable.prevV;
            string[] cc = syllable.cc;
            string v = syllable.v;
            string basePhoneme;
            var phonemes = new List<string>();
            if (syllable.IsStartingV)
            {
                basePhoneme = $"- {v}";
            }
            else if (syllable.IsVV)
            {  
                // if VV
                if (!CanMakeAliasExtension(syllable))
                {
                    //try VV
                    basePhoneme = $"{prevV} {v}";
                    //if no VV -> _V
                    if (!HasOto(basePhoneme, syllable.vowelTone))
                    {
                        basePhoneme = $"_{v}";
                        //if no _V -> V
                        if (!HasOto(basePhoneme, syllable.vowelTone))
                        {
                            basePhoneme = v;
                        }
                    }
                }
                else
                {
                    // the previous alias will be extended
                    basePhoneme = null;
                }
            }
            else if (syllable.IsStartingCV)
            {
                basePhoneme = $"{cc.Last()} {v}";
                for (var i = 0; i < cc.Length - 1; i++)
                {
                    phonemes.Add($"- {cc[i]}");
                }
            }
            else
            {
                // VCV
                basePhoneme = $"{prevV} {cc[0]}";
                if (HasOto(basePhoneme, syllable.vowelTone))
                    phonemes.Add(basePhoneme);
                basePhoneme = $"{cc.Last()} {v}";
                var offset = burstConsonants.Contains(cc[0]) ? 0 : 1;
                for (var i = offset; i < cc.Length - 1; i++)
                {
                    var cr = $"{cc[i]} -";
                    phonemes.Add(HasOto(cr, syllable.tone) ? cr : cc[i]);
                }
            }
            phonemes.Add(basePhoneme);
            return phonemes;
        }
        protected override List<string> ProcessEnding(Ending ending)
        {
            string[] cc = ending.cc;
            string v = ending.prevV;
            var phonemes = new List<string>();
            if (ending.IsEndingV)
            {
                phonemes.Add($"{v} -");
            }
            else
            {
                phonemes.Add($"{v} {cc[0]}");
                for (var i = 1; i < cc.Length; i++)
                {
                    var cr = $"{cc[i]} -";
                    phonemes.Add(HasOto(cr, ending.tone) ? cr : cc[i]);
                }
            }
            return phonemes;
        }
        // change rh V -> r V
        // since rh is a VC only alias, r is used as their natural approximant to make CV connections, if it happens
        protected override string ValidateAlias(string alias)
        {
            foreach (var vowel in vowels)
            {
                alias = alias.Replace("rh" + " " + vowel, "r" + " " + vowel);
            }
            return alias;
        }
        protected override double GetTransitionBasicLengthMs(string alias = "")
        {
            foreach (var c in shortConsonants)
            {
                if (alias.EndsWith(c))
                {
                    return base.GetTransitionBasicLengthMs() * 0.75;
                }
            }
            foreach (var c in longConsonants)
            {
                if (alias.EndsWith(c))
                {
                    return base.GetTransitionBasicLengthMs() * 1.5;
                }
            }
            return base.GetTransitionBasicLengthMs();
        }
    }
}
