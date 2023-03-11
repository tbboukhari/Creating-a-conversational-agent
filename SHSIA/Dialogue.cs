using System;                           // pour les entrées-sorties (Console)
using System.Collections.Generic;       // pour les List<T>
using System.IO;                        // pour la lecture et l'écriture de fichiers
using System.Text.Json;                 // pour la manipulation de fichiers JSON

namespace dialogue {

    /* Ce fichier contient deux classes : "Interface" et "Dialogue" */


    /* La classe "Interface" contient uniquement la méthode "agent_dialogue" et la méthode "display_emotion".
        C'est cette classe qui est différente dans la version Unity, tout le reste est inchangé.

        Pour la méthode "agent_dialogue" :
        - Dans la version texte, j'affiche les réponses numérotées et je boucle sur le numéro
        - Dans la version unity, on veut que l'agent dise la phrase, que les réponses proposées soient affichées
        en dessous sous la forme de boutons, et il faut cliquer sur une réponse.
        Dans les deux cas, la méthode termine en appelant Dialogue.handleResponse avec l'indice de la réponse choisie
        dans la liste des réponses proposées.

        Pour la méthode "display_emotion" :
        - Dans la version texte, on affiche juste le nom de l'émotion et l'intensité
        - Dans la version unity, il faudra jouer l'expression (ce sera l'objet du TP4)
    */
    public class Interface {
        /* L'attribut Dialogue permet de communiquer avec le dialogue */
        private Dialogue myDialogue;

        /* Le constructeur par défaut */
        public Interface(Dialogue d)
            => myDialogue = d;

        /* Cette méthode fait un tour de dialogue avec l'agent : l'agent dit une phrase puis propose N réponses sous forme de bouton.
           Les paramètres sont:
           - text: la phrase énoncée par l'agent
           - proposals: les contenus des boutons
           - values: les "code de retour" de chaque bouton */
        public void agent_dialogue(string text, List<string> proposals) {
            /* 0. Vérifications syntaxiques */
            if (proposals.Count==0) {
                Console.WriteLine("** Il y a une erreur dans votre code: la liste de proposition est vide.");
                Environment.Exit(0);
            }
            /* 1. Afficher le texte */
            Console.WriteLine();
            Console.WriteLine("** {0}",text);
            Console.WriteLine();
            /* 2. Afficher les réponses possibles */
            int i=1;
            foreach(string p in proposals) {
                Console.WriteLine("{0}. {1}",i,p);
                i = i+1;
            }
            Console.WriteLine();
            /* 3. Récupérer la réponse et renvoyer le code de retour associé */
            Console.WriteLine();
            while(true) {
                Console.Write(">> ");
                int r = Int32.Parse(Console.ReadLine());
                if (r<1 || r>proposals.Count)
                    Console.WriteLine("** Merci de répondre par une valeur entre 1 et {0}",proposals.Count);
                else {
                    Console.WriteLine();
                    Console.WriteLine();
                    myDialogue.handleResponse(r-1);
                    return;
                }
            }
        }

        /* Cette méthode permet d'afficher une émotion */
        public void display_emotion(string name, double intensity) {
            Console.WriteLine("==> Émotion *"+name+"* avec une intensité "+intensity);
        }
    }


    /*  La classe abstraite "Dialogue" est le support pour définir des dialogues.
        
        On définit un ensemble de variables.
        On définit un ensemble de questions déclenchées par les valeurs des variables
        Pour chaque question, on définit un ensemble de réponses.
        Pour chaque réponse, on définit les nouvelles valeurs des variables.
        Le dialogue fonctionne de la manière suivante :
        - sélection de la question (la première qui satisfait les conditions)
        - attente de la réponse (via la classe Interface)
        - modification des variables et appel de la méthode abstraite "action" dans la méthode handleResponse
        - recommencer tant que la méthode abstraite "done" ne renvoie pas true

        La méthode "readDialogueFile" permet de lire un fichier .json pour créer un dialogue. Il
        faut lui indiquer le type de sortie.

        La classe s'utilise simplement de la manière suivante :
        
        en écrivant une sous-classe S de Dialogue qui instancie
        les deux méthodes action et fini :
        
        class S : Dialogue {
            public override void action() {
                ...
            }
            public override bool done() {
                return ...
            }
        }
        
        puis en appelant les deux instructions suivantes :
        
        Dialogue d = Dialogue.readDialogueFile<S>("fichier.json");
        d.run();

        Dans l'écriture des méthode action et done, on peut utiliser les valeurs
        des variables qui sont manipulées par le dialogue. Pour cela, on dispose
        des méthodes getValue, hasValue et setValue.

    */
    public abstract class Dialogue {

        /************************************************/
        /* Structure C# du dialogue, alimentée via JSON */
        /************************************************/

        /* La classe pour les variables : un nom associé à une valeur */
        public class Variable {
            public string name {get; set;}
            public object val {get; set;}
        }

        /* La classe pour les réponses */
        public class Response {
            public string phrase {get; set;}
            public List<Variable> vars {get; set;}
        }

        /* La classe pour les questions */
        public class Question {
            public List<Variable> conditions {get; set;}
            public string phrase {get; set;}
            public List<Response> responses {get; set;}
        }

        /* Maintenant, un Dialogue est un ensemble de variables et de questions */
        public List<Variable> vars {get; set;}
        public List<Question> questions {get; set;}

        /**********************************/
        /* Méthodes de la classe Dialogue */
        /**********************************/

        /* Il faudra instancier la méthode abstraite "action",... */
        public abstract void action();
        /* ... la méthode abstraite "done"... */
        public abstract bool done();
        /* ... et la méthode getCurrentEmotion! */
        public virtual (string,double) getCurrentEmotion() {
            return ("neutral",0);
        }

        /* Une méthode bien pratique pour récupérer la valeur d'une variable du dialogue */
        public object getValue(string name) {
            foreach(Variable v in vars)
                if (v.name.Equals(name))
                    return v.val;
            Console.WriteLine("** Il y a une erreur dans votre dialogue ou dans votre code:");
            Console.WriteLine("** La variable {0} ne figure pas dans le dialogue",name);
            Environment.Exit(0);
            return null;
        }

        /* Une autre méthode bien pratique pour tester la valeur d'une variable du dialogue
            ATTENTION à la bidouille : on transtype tout en "string" pour comparer les valeurs !
        */
        public bool hasValue(string name, object val) {
            string current_value = getValue(name).ToString();
            string target_value = val.ToString();
            return current_value.Equals(target_value);
        }

        /* Une dernière méthode bien pratique pour modifier la valeur d'une variable du dialogue */
        public void setValue(string name, object val) {
            foreach(Variable v in vars)
                if (v.name.Equals(name)) {
                    v.val = val;
                    return;
                }
            Console.WriteLine("** Il y a une erreur dans votre dialogue ou dans votre code:");
            Console.WriteLine("** La variable {0} ne figure pas dans le dialogue",name);
            Environment.Exit(0);
        }

        /* La méthode pour lire un dialogue
            ATTENTION : T doit être une sous-classe de Dialogue
        */
        public static T readDialogueFile<T>(string filename) where T:Dialogue {
            string sdial = File.ReadAllText(filename);
            return JsonSerializer.Deserialize<T>(sdial);
        }

        /*************************/
        /* Déroulé d'un dialogue */
        /*************************/

        /* Les variables globales dont on a besoin pour le dialogue : l'interface et la question courante */
        private Interface gui;
        private Question current_q;

        // /* Cette variable contient le numéro de la réponse à chaque tour */
        // public int currentResponse {get; set;}

        /* La méthode run permet de dérouler un dialogue suivant l'algorithme proposé plus haut :
            - sélection de la première question qui satisfait les conditions
            - attente de la réponse
            - modification des variables
            - appel de la méthode abstraite "action"
            - recommencer tant que la méthode abstraite "done" ne renvoie pas true
         */
        public void run() {
            /* -1. créer l'interface de dialogue */
            gui = new Interface(this);
            /* 0. Test de fin */
            while (!done()) {
                current_q = null;
                /* 1. Sélection de la question */
                foreach(Question q in questions) {
                    /* vérification des conditions */
                    bool ok = true;
                    foreach(Variable v in q.conditions) {
                        if (!hasValue(v.name,v.val)) {
                            ok = false;
                            break;
                        }
                    }
                    /* si toutes les conditions sont satisfaites, on part sur cette question */
                    if (ok) {
                        current_q = q;
                        break;
                    }
                }
                /* car d'erreur : aucune question possible */
                if (current_q == null) {
                    Console.WriteLine("** Il y a une erreur dans votre dialogue:");
                    Console.WriteLine("** Aucune question ne satisfait les conditions courantes:");
                    foreach(Variable vv in vars)
                        Console.WriteLine("**   {0} = {1}",vv.name,vv.val);
                    Environment.Exit(0);
                }
                /* 2. Affichage de la question récupération de la réponse */
                List<string> props = new List<string>();
                List<int> vals = new List<int>();
                int i=0;
                foreach(Response rep in current_q.responses) {
                    props.Add(rep.phrase);
                    vals.Add(i);
                    i=i+1;
                }
                gui.agent_dialogue(current_q.phrase,props);
                /* la suite est dans handleResponse */
            }
        }

        /* Cette méthode permet de gérer la réponse */
        public void handleResponse(int r) {
                /* 3. Application des modifications sur les variables */
                Response current_r = current_q.responses[r];
                foreach(Variable v in current_r.vars)
                    setValue(v.name,v.val);
                /* 4. appel de la méthode abstraite "action" */
                action();
                /* 5. Expression de l'émotion courante (pourrait être mise en début de cycle) */
                (string nom, double intensite) = getCurrentEmotion();
                gui.display_emotion(nom,intensite);
        }

    }

}