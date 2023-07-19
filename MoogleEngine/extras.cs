
namespace MoogleEngine;
public static class Extras
{

    //metodo que devuelve un diccionario de cada documento y su titulo
    public static Dictionary<string, string> Title_Of_Each_Document(string[] documents)
    {
        Dictionary<string, string> Title_Of_Each_Document = new Dictionary<string, string>();
        string title;
        string document;
        for (int i = 0; i < documents.Length; i++)
        {
            title = documents[i].Substring(documents[i].LastIndexOf(@"\") + 1);
            document = File.ReadAllText(documents[i]);
            Title_Of_Each_Document.Add(document, title);
        }
        return Title_Of_Each_Document;

    }
    //metodo que normaliza los textos quitando mayusculas(ToLower),
    // signos de puntuacion()
    public static void Normalize_Text(List<string> example)
    {
        foreach (string item in example)
        {
            //para eliminar las mayusculas
            item.ToLower().ToList();
            //para eliminar las tildes, formD(separar los caracteres con tildes y mantener los caracteres bases)
            item.Normalize(System.Text.NormalizationForm.FormD);
            //para eliminar las comas y los puntos(. , : ;)
            item.Replace(",", " ");
            item.Replace(".", " ");
            item.Replace(";", " ");
            item.Replace(":", " ");
            //dejo solamente letras y numeros en la lista
            foreach (char index in item)
            {
                if (!Char.IsLetterOrDigit(index))
                {
                    item.Replace("index", " ");
                }
            }
        }

    }
    //metodo que analiza la cantidad de veces que se repite
    // una palabra en un documento(TF) 
    public static double Term_Frequency(List<List<string>> documents, string example)
    {
        double count = 0;
        for (int i = 0; i < documents.Count; i++)
        {
            for (int j = 0; j < documents[i].Count; j++)
            {
                if (documents[i][j] == example)
                {
                    count += 1;
                }
            }
        }
        return count;
    }
    //metodo que calcula la cantidad de docuemntos en los que aparece una palabra del vocabulario
    public static double Count_Documents_By_Word(List<List<string>> documents, string example)
    {
        double count = 0;

        for (int i = 0; i < documents.Count; i++)
        {
            for (int j = 0; j < documents[i].Count; j++)
            {
                if (example == documents[i][j])
                {
                    count += 1;
                    break;
                }
            }
        }

        return count;
    }
    //metodo para saber la cantidad de documentos
    public static double Count_Of_Documents(List<List<string>> documents)
    {
        double count = documents.Count;
        return count;
    }
    // metodo que calcula el idf de cada palabra en cada documento
    private static double Inverse_Document_Frencuency(List<List<string>> documents, string example)
    {
        double Result = 0;
        double Cantidad_Documentos = Count_Of_Documents(documents);
        double Cantidad_Documentos_Por_Palabras = Count_Documents_By_Word(documents, example);
        Result = Math.Log(Cantidad_Documentos / Cantidad_Documentos_Por_Palabras);
        return Result;
    }
    //metodo que devuelve un diccionario con el idf de todas las palabras.
    public static Dictionary<string, double> Convert_To_Diccionary_IDF(List<string> vocabulary, List<List<string>> documents)
    {
        double IDF = 0;
        int count = 0;
        for (int i = 0; i < vocabulary.Count; i++)
        {
            IDF = Inverse_Document_Frencuency(documents, vocabulary[i]);
            count = i;
        }
        var Dictionary_IDF = new Dictionary<string, double>();
        Dictionary_IDF.Add(vocabulary[count], IDF);
        return Dictionary_IDF;

    }
    //metodo que devuelve una lista de lista donde cada sublista es un documento y cada elemento
    //de cada lista es el tf por el idf de las palabras de ese documento
    public static List<List<double>> Matrix_TF_IDF(List<List<string>> documents, List<string> Vocabulary)
    {
        List<List<double>> Matrix_Result = new List<List<double>>();
        double tf = 0;
        var idf = Convert_To_Diccionary_IDF(Vocabulary, documents);
        for (int i = 0; i < Vocabulary.Count; i++)
        {
            tf = Term_Frequency(documents, Vocabulary[i]);
            Matrix_Result[i].Add(tf * idf[Vocabulary[i]]);
        }
        return Matrix_Result;

    }
    //metodo que calcula el tf del query
    public static List<double> TF_Query(List<string> query, List<string> vocabulary)
    {
        double count = 0;
        List<double> Query_TF = new List<double>();
        for (int i = 0; i < vocabulary.Count; i++)
        {
            for (int j = 0; j < query.Count; j++)
            {
                if (vocabulary[i] == query[j])
                {
                    count += 1;
                }
                if (j == query.Count - 1)
                {
                    Query_TF[i] = count;
                }
            }
        }
        return Query_TF;
    }
    //guardamos en un lista el idf del query
    public static List<double> IDF_Query(List<string> query, Dictionary<string, double> IDF_Word)
    {
        List<double> Query_IDF = new();
        for (int i = 0; i < query.Count; i++)
        {
            Query_IDF.Add(IDF_Word[query[i]]);
        }
        return Query_IDF;
    }
    //empezamos con el calculo de la similitud de coseno, para eso empezamos calculando la magnitud de los vectores
    public static double Vector_Magnitude(List<double> Matrix_TF_IDF)
    {
        double result = 0;
        for (int i = 0; i < Matrix_TF_IDF.Count; i++)
        {


            result += Matrix_TF_IDF[i] * Matrix_TF_IDF[i];
        }
        return Math.Sqrt(result);
    }


    //metodo para calcular la similitud de coseno entre un documento y el query
    public static double Cosine_Similarity(List<double> doc1, List<double> query, double Magnitude_Doc1, double Magnitude_query)
    {
        double Similitud_Coseno = 0;
        double valor_vectores = 0;
        for (int i = 0; i < doc1.Count; i++)
        {
            valor_vectores += doc1[i] * query[i];
        }
        Similitud_Coseno = valor_vectores / (Magnitude_Doc1 * Magnitude_query);
        return Similitud_Coseno;
    }
    //metodo que devuelve la matriz del snippet de un documento donde cada sublista es una oracion de del docuemnto
    public static List<List<string>> Matrix_Snippet(List<string> Document)
    {
        List<List<string>> Snippet = new List<List<string>>();
        string Actual_document;
        string[] Document_Split_Into_Sentences;
        for (int i = 0; i < Document.Count; i++)
        {
            for (int j = 0; j < Document.Count; j++)
            {
                Actual_document = Document[j];
                Document_Split_Into_Sentences = Actual_document.Split(".");
                Snippet[i][j] = Document_Split_Into_Sentences[j];
            }
        }
        return Snippet;
    }
    //metodo que devuelve una lista de lista del tf por el idf del snippet
    public static List<List<double>> Snippet_TF_IDF(List<List<string>> Snippet, List<string> Vocabulary)
    {
        List<List<double>> Snippet_TF_IDF = new List<List<double>>();
        double Tf = 0;
        var Idf = Convert_To_Diccionary_IDF(Vocabulary, Snippet);
        for (int i = 0; i < Snippet.Count; i++)
        {
            Tf = Term_Frequency(Snippet, Vocabulary[i]);
            Snippet_TF_IDF[i].Add(Tf * Idf[Vocabulary[i]]);
        }
        return Snippet_TF_IDF;
    }
    //metodo que devuelve un diccionario de los documentos a devolver con usu titulo
    public static Dictionary<string, string> Documents_with_Title(List<string> Documents, Dictionary<string, string> Document_Title)
    {
        Dictionary<string, string> Documents_with_Title = new Dictionary<string, string>();
        for (int i = 0; i < Documents.Count; i++)
        {
            Documents_with_Title.Add(Documents[i], Document_Title[Documents[i]]);
        }
        return Documents_with_Title;

    }
    //metodo que devuelve un array de searchItem de cada documento a devolver
    public static SearchItem[] Result(string[] title, double[] score, string snippet, List<string> Return_Documents)
    {
        List<SearchItem> result = new List<SearchItem>();
        SearchItem Preresult;
        for (int i = 0; i < Return_Documents.Count; i++)
        {
            Preresult = new SearchItem(title[i], snippet, score[i]);
            result.Add(Preresult);
        }
        return result.ToArray();

    }






}






