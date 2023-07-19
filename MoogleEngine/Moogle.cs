namespace MoogleEngine;


public static class Moogle
{
    public static SearchResult Query(string query)

    {
        SearchItem[] items = new SearchItem[] { };

        //guardar las direcciones de los documentos. 
        //path.join concatena varios elementos unidos de un (/)
        //directory.getfiles devuelve una cadena con todas las direcciones separados por el .txt.

        string Address = Path.Join(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName, "content");
        string[] Documents_Address = Directory.GetFiles(Address, ".txt*");
        string Title = Address.Substring(Address.LastIndexOf(@"\") + 1);

        // guardar en un array de string todos los documentos
        string[] Documents = new string[Documents_Address.Length];
        for (int i = 0; i < Documents_Address.Length; i++)
        {
            Documents[i] = File.ReadAllText(Documents_Address[i]);
        }

        // creo una lista de lista donde cada sublista es un documento 
        List<List<string>> All_Documents = new();
        for (int i = 0; i < Documents.Length; i++)
        {
            All_Documents[i].Add(Documents[i]);

        }
        //creo un diccionario de los titulos con sus documentos
        Dictionary<string, string> Title_Of_Each_Document = Extras.Title_Of_Each_Document(Documents_Address);

        // creo una lista que guarde todas las palabras de los documentos creando asi el Vocabulario.
        List<string> Vocabulary = new();
        for (int i = 0; i < Documents.Length; i++)
        {
            Vocabulary.AddRange(All_Documents[i]);
        }
        //Metodo que normaliza la lista, quita signos de puntuacion, mayusculas, tildes, dejo solo letras y numeros 
        Extras.Normalize_Text(Vocabulary);
        //elimino los elementos repetidos de la lista
        Vocabulary.Distinct();


        //guardamos el query en una lista y lo normalizamos
        List<string> Query = query.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        Extras.Normalize_Text(Query);
        //creamos un diccionario que seran el idf de todas las palabras del vocabulario
        var Dictionary_IDF_Words = Extras.Convert_To_Diccionary_IDF(Vocabulary, All_Documents);
        //guardamos en lista el tf y el idf del query para luego multiplicarlo y crear asi nuestro vector query
        List<double> TF_Query = Extras.TF_Query(Query, Vocabulary);
        List<double> IDF_Query = Extras.IDF_Query(Query, Dictionary_IDF_Words);
        List<double> TF_IDF_Query = new();
        for (int h = 0; h < TF_Query.Count; h++)
        {
            TF_IDF_Query.Add(TF_Query[h] * IDF_Query[h]);
        }

        //creamos una lista  de lista que sera nuestra matriz con el tf por el idf de cada palabra en cada documento
        List<List<double>> matrix_TF_IDF = new List<List<double>>();
        //matrix_TF_IDF.Add(Extras.Extras.Matrix_TF_IDF(All_Documents, Vocabulary));


        //calculamos las magnitudes de los vectores(documentos) para empezar a calcular la similitud de coseno
        double Norm = 0;
        for (int k = 0; k < matrix_TF_IDF.Count; k++)
        {
            Norm = Extras.Vector_Magnitude(matrix_TF_IDF[k]);
        }
        //le calculamos la magnitud al query
        double Norm_Query = Extras.Vector_Magnitude(TF_IDF_Query);
        //guardamos en un diccionario el resultado de la similitud de coseno con su respectivo documento
        Dictionary<double, string> Dictionary_PreScore = new Dictionary<double, string>();
        double Score = 0;
        List<double> Score_Docs = new();
        for (int m = 0; m < All_Documents.Count; m++)
        {
            for (int j = 0; j < All_Documents.Count; j++)
            {
                Score = Extras.Cosine_Similarity(matrix_TF_IDF[j], TF_IDF_Query, Norm, Norm_Query);
                Score_Docs.Add(Score);
                Dictionary_PreScore.Add(Score, All_Documents[m][j]);
            }


        }
        //ordeno la lista  del score de mayor a menor y  guradamos en un diccionario los 
        //documento con su score de mayor a menor
        Score_Docs.Sort();
        Dictionary<string, double> Dictionary_Score = new Dictionary<string, double>();
        for (int p = 0; p < Score_Docs.Count; p++)
        {
            Dictionary_Score.Add(Dictionary_PreScore[Score_Docs[p]], Score_Docs[p]);
        }
        //creamos una lista que seran los documentos que devolveremos de mayor a menor
        List<string> Return_Documents = Dictionary_Score.Keys.Take(10).ToList();
        //creamos uns lista de lista para el snippet, donde cada sublista sera una oracion del documento
        //cuales documentos seran la lista creada anteriormente de los documentos devueltos
        List<List<string>> Snippet = Extras.Matrix_Snippet(Return_Documents);
        //tenemos ahora la matriz del snippet con el tf y el idf calculado
        List<List<double>> Snippet_TF_IDF = Extras.Snippet_TF_IDF(Snippet, Vocabulary);
        //calculamos la magnitud de los vectores(oraciones) del snippet 
        double Norm_Snippet = 0;
        for (int i = 0; i < Snippet_TF_IDF.Count; i++)
        {
            Norm_Snippet = Extras.Vector_Magnitude(Snippet_TF_IDF[i]);
        }
        //calculamos la similutud de coseno entre el snippet, guardando en un diccionario la oracion con su
        // respectivo score
        Dictionary<double, string> Pre_Snippet_Score = new Dictionary<double, string>();
        double Score_Snippet = 0;
        List<double> Score_Snippet_List = new();
        for (int i = 0; i < Snippet.Count; i++)
        {
            for (int j = 0; j < Snippet.Count; j++)
            {
                Score_Snippet = Extras.Cosine_Similarity(Snippet_TF_IDF[j], TF_IDF_Query, Norm_Snippet, Norm_Snippet);
                Score_Snippet_List.Add(Score_Snippet);
                Pre_Snippet_Score.Add(Score_Snippet, Snippet[i][j]);
            }
        }
        //ordeno la lista de mayor a menor y guardo en un diccionario las oraciones con su score de mayor a menor
        Score_Snippet_List.Sort();
        Dictionary<string, double> Score_Snippet_Oficial = new Dictionary<string, double>();
        for (int i = 0; i < Score_Snippet_List.Count; i++)
        {
            Score_Snippet_Oficial.Add(Pre_Snippet_Score[Score_Snippet_List[i]], Score_Snippet_List[i]);

        }
        //creamos un string con la oracion que mas score tenga y esa sera la que entreguemos 
        List<string> Finalist = Score_Snippet_Oficial.Keys.Take(1).ToList();
        string snippet = Finalist[0];
        string[] titles = new string[Return_Documents.Count];
        double[] score = new double[Score_Docs.Count];
        //diccionario que tiene los titulos de los documentos a entregar
        Dictionary<string, string> Documents_Title_Deliver = Extras.Documents_with_Title(Return_Documents, Title_Of_Each_Document);
        for (int i = 0; i < Score_Docs.Count; i++)
        {
            titles[i] = Documents_Title_Deliver[Return_Documents[i]];
            score[i] = Score_Docs[i];
        }
        SearchItem[] Almost_Last = Extras.Result(titles, score, snippet, Return_Documents);




















        return new SearchResult(Almost_Last,);
    }
}
