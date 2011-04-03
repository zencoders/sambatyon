using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Persistence
{
    /// <summary>
    /// Classe Factory che si occupa della creazione dei TrackRepository in modo da mascherare le classi specializzate.
    /// Le classi create per essere richiamate dalla Factory devono essere contenute nel namespace Persistence.Repository e 
    /// devono chiamarsi <c><i>NomeTipo</i>TrackRepository</c> e devono avere un attributo costante chiamato RepositoryType 
    /// contenente una stringa che identifica il tipo del repository.
    /// Il tipo del repository viene usato dalla Factory per generarne un'istanza.
    /// </summary>
    /// <seealso cref="Persistence.TrackRepository"/>
    public static class TrackRepositoryFactory
    {
        /// <summary>
        /// Metodo privato che genera il nome della classe del Repository in base al suo tipo.
        /// </summary>
        /// <param name="repType">Il tipo di repository</param>
        /// <returns>La stringa contenente il nome completo della classe implementante il repository del tipo richiesto.</returns>
        private static String _generateRepositoryClassName(String repType) {
            return "Persistence.Repository." + repType + "TrackRepository";
        }
        /// <summary>
        /// Metodo statico per ottenere un'istanza di un repository di un tipo specificato.
        /// Se tutto va a buon fine il metodo ritornerà un oggetto delle classe Persistence.Repository.TipoTrackRepository derivata
        /// dalla classe astratta Persistence.TrackRepository.
        /// Se viene richiesto un tipo di repository non disponibile verrà sollevata un'eccezione; è quindi preferibile, nel caso in 
        /// cui non si sia sicuri della disponibilità di un tipo di repository, usare il metodo IsValidRepositoryType della Factory.
        /// </summary>
        /// <example>
        /// Persistence.TrackRepository repo=Persistence.TrackRepositoryFactory("raven");
        /// Console.Writeline(repo.RepositoryType); //raven
        /// </example>
        /// <param name="repType">Stringa rappresentante il tipo di repository richiesto.</param>
        /// <returns>Un'istanza del repository del tipo richiesto.</returns>
        /// <exception cref="System.TypeLoadException">Eccezione sollevata nel caso ci siano errori nel caricamento della classe 
        /// del repository o nell'invocazione del suo costruttore di default.</exception>
        /// <exception cref="System.ArgumentException">Eccezione sollevata nel caso in cui la stringa contenente il tipo di repository
        /// richiesto risulti vuota</exception>
        /// <exception cref="System.ArgumentNullException">Eccezione sollevata nel caso in cui la stringa contenente il tipo di
        /// repository richiesto risulti essere nulla</exception>
        public static TrackRepository GetRepositoryInstance(String repType)
        {
            if (repType != null)
            {
                if (repType.Length != 0)
                {
                    String className = _generateRepositoryClassName(repType);
                    try
                    {
                        Type reflectedRepository = Type.GetType(className, true, true);
                        object rep = reflectedRepository.GetConstructor(Type.EmptyTypes).Invoke(null);
                        return rep as TrackRepository;
                    }
                    catch (Exception ex)
                    {
                        throw new TypeLoadException("Unable to load repository class " + className, ex);
                    }
                }
                else
                {
                    throw new ArgumentException("Repository Type Name must not be Empty!");
                }
            }
            else
            {
                throw new ArgumentNullException("Repository Type Name must not be NULL!");
            }
        }
        /// <summary>
        /// Metodo statico utilizzato per sapere se è disponibile una classe rappresentante il repository con il tipo specificato.
        /// </summary>
        /// <param name="repType">Tipo di repository richiesto</param>
        /// <returns>Ritorna <c>true</c> se il tipo di repository specificato esiste, altrimenti <c>false</c></returns>
        public static  bool IsValidRepositoryType(String repType)
        {
            if ((repType != null) && (repType.Length != 0))
            {
                String className = _generateRepositoryClassName(repType);
                Type reflectedRepository = Type.GetType(className, false, true);
                return (reflectedRepository != null) ? true : false;
            }
            else
            {
                return false;
            }
        }
    }
}
