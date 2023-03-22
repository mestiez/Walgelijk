namespace Walgelijk.Onion.Assets;

public static class BuiltInAssets
{
    public static readonly FixedAudioData Click = new(
        Convert.FromBase64String("UklGRjwOAABXQVZFZm10IBAAAAABAAEARKwAAIhYAQACABAAZGF0YRgOAAD//wIA/f8CAP7///8CAP//AwD8//7//f8BAPn/CAABAPv/FQAnABM" +
            "AMwAOAOb/EQB4/yz/SwDb/9P/QAEYAIr/rwDU/8j/mgCZ/x7/3/+6/24A7AExAbP/9f2E/i4A2/4cAQ0CvP2s/+4B6f9ZAVEB/v1T/t3/fP/VADoBVv6D/qwA1/99AQUDsf8R" +
            "/x8AiP9wAIAAIf/M/oz/swCWAVYBy/97/oL9TP/uAGr/igH/ABL9DQFbAer9PwEBAIH9xQHuASgAuwF1/+r8Jf8cAJD/sgGrAYH/vwAdAbH/HgCR/97+BgBZAJoAr/8h/tj/hv" +
            "/C/7sCtQH1/47/iP/N/qH++ADXAqsBfP+3ANf+LPyl/yABHQAR/08AJf+F/J0AhP6w/r8F3P9b/5sERvvs+pgC2f2Y/98FggAW/ikAQvom+iwF4gMoAs8JigAe+Jj9lvys/jAG" +
            "8Qb3AcD96f1O+Zb6HQMVArUFuQVp/Fz7Hfzn/qgArAWZBxD/gf5G+q36tAH3AkP/hvzFByD89vraC0L+XQCe/SD6lQRW+0AElwnY/H37HAKHBAH6WAJzCzL6svSOAGIB5PsTA6" +
            "kGmv40/X//W/4SATMCUP/WALH+tftL/7UAzP4J/1IBrP5n/ZYAKABYALL+8gCtAKT4/P+T/an2hAT2ADH/6AaoAjsEQwIbBKsEV/sW+pf07PIB+ZH7TgL1BYQEtQA+/on/pP5f" +
            "BOMKvgTc/sv9S/mb+cUAbwSsA5ABNwPz/6X56/3X/Or5hwLjBTQBfgNPBEgAfgmVBSz6t/1A7LPpAQExAd8HWBRNCev9swS1BWr+lQZJAZ7wouj83J7uphbAIFATLQmT/tvtHeJ" +
            "G51AFWB/HFK4EeQSL+H7r3v9WE2wDJvl0CjoOfPbQ6qz1p/6pADQIzxSxF3H+gdFtzFP8TCC4G4cPfwsu+c/ndffWELkVfQpE+Tr0MAEv/Uf+1Q1D8GTVTuyfCegbgQ87+XAAJg" +
            "eQ/xzxUOpBBf0fLRUZ92LhWu7YDEMX/wyRBrweDSLI+oXjPfa2ILP82adWrRoJEGoTUnPzqNTG3/fvgeQe8rgkETJuE8HdBt/WBdQKOwSN+d727gLZ+ZveWuh+C8sgaiLtCZL6vv" +
            "uJ8UbngfJUEhcgcPssy8bhrS33RfgVntamys74thipINcfWwo6+iXi08/r7wUWySYLFA/f6NdO8Qj7kA25CqPykOcz7zoRpC2DLAn5ssbN1u32NhtzHGb/uwknDREJav0t7hUHgB" +
            "N7D88I4P4wBEHvmNNR4lsTWjjQF+Dg09R77lQMwQKJ9R4USiwlGNb4TOqs+kUOHPle8QoLnxStG8AVvuQjpguPjcUeK8Vr1FKUCb3gG+G16IT3VwjZEqwowifo/wXs/+yw6ATh6t5" +
            "O/NgrGzvZEu/gCt1n74kDABOAF5MnwyG+9zbWPNGE8B4SNhquG1MZXP+H1JjFxdP8+Jsigh3VEmAQ3PiY8hTuqOmrArsPiA3nDZgDWPtY+f72a/8KCq0KfgpdCA0CZf/2AP8B0gUv" +
            "CksJ3A4CElER7g9h376tlMDj3w4HkCu4H+4ceRAp4SreGfndDlcl7CDWCF7/+vkL7SLsZfR5AQQVkxLu+i/qhunA9TL8yf44ER8drBMLBs72dPMf+kDyM/PCBnUP/BD/CUr05u0K9" +
            "j34qgM8DxMJnwSRBKAAOP0o+Az2Z/y9Bw8LmQIM/4n8qvqTALP/kwJfCCYBkAAOA7b+8QBMAsH+C/2f+un7OgXKC/sIpQcEAzb2b/nkA0ECw/0t9sH5JAPv+Cbznf2xAWv+8vwZ/" +
            "gUBi//Y/FIFKwpgBdADZAGx/m/9Ef3SAccGpQKZ+WD6WAL8BusDbPxB/zoFb/yK92cAawMJABL74/s0BtwGvwJpBKQDHAF8/jIAdQavB1kCUP3h/YL9lfaQ8zf60gGiBFIBBvui+" +
            "2gB8wLRARsAlv//BVoMqAgDAZj7qPik+T/8//6tBZUKfgaGANL7ivgk/VkDDwOYAc7/o/71AS4DCwHcAFT//Pxw/Xz9w/08/xEBigQkBYsA5ft/+rj8ff5//p0BaARqA9QCRAHY/u" +
            "r/tABo/3X+xvzw/EcBPwRgAjX/sPzl+8z95P+fASwD/AIHAbL/QQArAL3/VwBIAN4BygMjAh4Agv7++5/7VP44AQ4DtwSrBIABDv7p+wv9mP/w/sr9o/5IAGsChwOXAvn/GP4J/04" +
            "AOwFzAcoA8wFyAuUAwP7G/Az9dP5S/17/NP8g/8b9Pf0+/n3+ev4I/wsAzgAKAQIC7wIjAhwAOP+S/6gAMgJDA2ACOQDs/zABKQH1/73+uP65/54AqwEpASf/NP6n/XP8EPyC/QYA" +
            "dAF/APf+k/5o/ir+U/4k/8IAZgIFAoP/lv7n/4IBsgJuAq0BdwEtAbcABQDU/4EAfwHyAUMBIAD//nL+Cv+t/5v/T/93/hn95/zC/en+HwBFAE//Av6G/Qf/GQH1AZoB1gACALf/fg" +
            "DmAaUCcgLsAfgAGwD9/1sArADEAJsA+f+7/rr9QP6g/3gApAAyACD/W/6y/j0A+ADi/3/+0P0w/rD/tQBmALL/Pf9l//v/hgBCARkC7gH2ADkAtv/4/7sAFAHoAGgABQDw/9T/2f/s" +
            "/7b/Z/9r/93/EQDu//7/SADI/6X+bP5A/ykARQB4///+6P4U/0z/j/8aAIcAwgDiAOYABgHAABgAxP82ABoBbgEgAcMAfQAVAKj/jP/v/zcANQD4/4H/Jv8o/4D/8/8vANr/3/75/T" +
            "f+Sf/9/7r/E//z/mj/vv+j/8z/TgDMAO0A3AAJAWUBPAFQAI//m/8mAJkAgQAfAMP/Q//W/rn+F//H/x4AHwAEAP7/zv93/1v/vP83ADkA+f++/8n/zf9//yj/IP80/zH/Tv+r/+//6" +
            "v+f/4f/1/8vADIAFAAXAFwAnQCqAIYASQDv/2T/B/8q/6n/9P/c/57/j/+7/83/uP+0/9P/wP9z/1f/z/9nAH8ADQCU/4X/j/+G/5f/9f9hAJUATQADABYAUgBYAFsAWABfAEAACQAJ" +
            "AEUAawAwAMv/hv+N/9H/DwA3AD8AEACy/3X/hP/X/yIAJADt/7n/r//Z/yIAXABOAPf/qv/D/zwArACzAHUAVAByAIUAXgAyAC0AOAAaAPP/zv/Y/+b/9/8XADAAKwD4/+3/GQBaAFQ" +
            "AGwD6/wcAGwAlACIAIAAjAPz/0f/i/xkAQABAACMACQAdABcAHAAtAEYANwDx/6T/mv/a/x4ANAAlACUAKQAPAPT/CwBBAFMAJADy/+7/8f/c/5j/b/99/6b/nP9w/1P/aP+Q/7D/wf/" +
            "q/wIACQAaAE0AawBbADMACADt/7//lf+B/4n/fv9X/zT/Tf+r//v/EQD2//j/EgAYAP7/5f/+/xwAFAD6/+7/5P+6/4v/Yf90/5n/wv/g/wwAIwA6ADwAXACGAJMAgABVADgAJgAIANT/p" +
            "/+b/5T/iv+G/53/yv/i/+L/6f8hAFMAhgB6AFwAMwAPAPz/AQAIAA0A5P/e/+L/z//x/93/tv/c//b/9P8gACwA+v/a/7z/p//t/zsAQABjADsAvP9//4H/oP/z/zwANAD0/8v/pv+R/7" +
            "n/BABDAHoAOADG/5D/lv/L////PgAlAPb/wf+h/9D/HQArAAQAKAAhAPD/KAAkAPX/3v+6/9//5P8CAFUAQAD7//j/IwAPAA8AXQA+AML/uf/q/+3/+/81ACkABADx//3/BAAiABgAGwA" +
            "RAPb/8v8DAAwAAgALAAMA9P/4/wUABgAAAP7/BgDd/9b/4P+//97/DQALACkANQA2ACwAKgA1AAUAzf+R/2f/dP+n/+X/LQBCADAABQD0/+//DwBRAGAAMAD1/8n/rv/P/w8AMQArACUA" +
            "EwDi/87/0v/I/+f/IQAyACgAMgAgAD4AUQAQANz/df8O/13/2v88AKwAuwBcACAALwAYACQAJQC9/yn/mv6o/s3/EQFnAfUAXACZ/93+ov5Q/5cAOwHoAGYA7/9h/3f/NACBACAAIQB2A" +
            "CcAe/8+/4j/4P86AKsADQGkACX/2P1a/h8AUQFXAecANgBb/zD/7v/AAOAAVACt/6//0v/n/zsA8v/Y/ov+aP+fABYBgwACABIAFgCy/z//hf+cADIBlgBc/97+jv+YAO4AuQD1AGoB" +
            "1wCP/x//IgB5AFX+Wfyr/bIBLgSgAqb/Mv5Y/qb+Bv9VAMoB2wEyALj+BP/4/10AFQDF/8P/0P8z/7r+eP/GAI4BSQF2AND/eP8Y/wr/5f/2ALoAEv8//tv/MQJ6AkkAIv4z/sj/K" +
            "AHDAVUBYgA7/y3+QP6q/zYBiwEuAKf+bP4g/wAAjAAcAF7/Cv/H/zQBEgI3AQv/w/1T/uz/OQHuAIIAlQB6ADgAoP+v/3AAxQDEAGMAEwCk/7D+S/5X/2gBAAJhAJH+Xv5k/x0AEAB" +
            "eAFEBngHDAJX/Uf/q/xMAtP/j/40AIQFBARIAlv1S+3L7qf7ZArQELwNQAKP+T/7e/sn/lgBiAf0BOQHn/yD/0v6a/nD+Bv+pADYCFQI/AJ7+fP5E/14AEwGiAdMB2wAr//D9PP6t/+" +
            "MAbQFxAbkALP+6/Vr9YP45AHgBdgEPAVkAl/8q//z+cP86ALcA0ACGACoAxf+Q/7D/+/9VAJUAhgBiADcADQAKADEAaQB3AK0AywDKAKkAgv+F/ar8YP3w/uQA4wHNAVAB5f+s/sf+1" +
            "P8SAboBTAF7AOL/Xv8K/yb/mv9sAOUAggCZ//v+Cv95/9P/YAAIAT8BywANAPb//f/7/wEA//8DAP3/AQD+/w=="),
            44100, 1, 1804
        );

}
