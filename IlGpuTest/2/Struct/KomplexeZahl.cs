namespace IlGpuTest._2.Struct
{
    public readonly struct KomplexeZahl
    {
        public readonly float ReelerTeil;
        public readonly float ImaginaererTeil;

        public float Absolut => ReelerTeil * ReelerTeil + ImaginaererTeil * ImaginaererTeil;

        public bool GehtGegenUnendlich => Absolut >= 4;

        public KomplexeZahl()
        {

        }

        public KomplexeZahl(float real, float imaginaer)
        {
            ReelerTeil = real;
            ImaginaererTeil = imaginaer;
        }

        public static KomplexeZahl operator *(KomplexeZahl a, KomplexeZahl b)
        {
            var resultReel = a.ReelerTeil * b.ReelerTeil - a.ImaginaererTeil * b.ImaginaererTeil;
            var resultImaginaer = a.ReelerTeil * b.ImaginaererTeil + a.ImaginaererTeil * b.ReelerTeil;

            return new KomplexeZahl(resultReel, resultImaginaer);
        }

        public static KomplexeZahl operator +(KomplexeZahl a, KomplexeZahl b)
        {
            var resultReel = a.ReelerTeil + b.ReelerTeil;
            var resultImaginaer = a.ImaginaererTeil + b.ImaginaererTeil;

            return new KomplexeZahl(resultReel, resultImaginaer);
        }
    }

}
