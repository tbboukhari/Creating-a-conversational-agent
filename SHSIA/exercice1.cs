using dialogue;  

namespace exercice1 {  
    class Test : Dialogue {
        public override void action() {
         // c'est vide pour l'instant
            if (!hasValue("etat","fin"))
                setValue("etat", "debut");
        }
        public override bool done() {
        // c'est vide pour l'instant
            return hasValue("etat","fin");
        }
        /* la méthode de lancement */
        static void Main(string[] args) {
            Dialogue d = readDialogueFile<Test>("dialogue0.json");
            d.run();
        }
    }
}