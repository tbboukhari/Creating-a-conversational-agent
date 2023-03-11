using dialogue;
using System;
using System.Globalization;
namespace swahili {

    /* Cette classe regroupe les constantes utilisées par la classe Swahili */
    static class Constants
    {
        /* Cette constante définit la probabilité de poser la question sur l'humeur à chaque
           tour du dialogue */
        public const double Freq_Humeur = 0.5;
        public const double ParametreAgent =0.5;
        /* Cette constante est un générateur de nombre aléatoire, utilisé dans la classe
           de dialogue */
        public static Random rand = new Random();

    }

    class Swahili : Dialogue {

        public override void action() {

            /* pour apprendre un nouveau mot, on incrémente la variable */
            if (hasValue("etat","appri")) {
                try {
                    int v = Int32.Parse(getValue("prochain_mot").ToString());
                    setValue("prochain_mot",v+1);
                }
                catch (FormatException)
                {
                    Console.WriteLine($"Erreur dans le fichier JSON : impossible de convertir '{getValue("prochain_mot").ToString()}' en entier");
                }
                setValue("etat","debut");
            }

            /* pour apprendre un nouveau mot, on incrémente la variable */
            if (hasValue("etat","reciter")) {
                int r = 1+Constants.rand.Next(6);
                setValue("tirage",r);
            }

            /* de manière aléatoire, on glisse le dialogue sur l'humeur */
            if (hasValue("raison","ignore") && Constants.rand.NextDouble()<Constants.Freq_Humeur) {
                setValue("sympa",false);
                setValue("humeur","inconnue");
                setValue("raison","inconnue");
            }
            /* et quand la question a été posée, on la remet à ignore */
            //else if (!hasValue("humeur","ignore")&&!hasValue("raison","inconnue")) {
                // en réalité, avant de les supprimer, il faudra faire quelque chose de ces valeurs...
                /*
                setValue("humeur","ignore");
                setValue("raison","ignore");
                */
            //}
            //********************Exercice4**************************
            /*
            if (hasValue("correct",true) && hasValue("etat","feedback")){
                //setValue("desire",0.7);
                //setValue("time","past");
                //setValue("cause","user");
                int v = Int32.Parse(getValue("appri").ToString());
                setValue("appri",v+1);
            }
            if (hasValue("correct",false) && hasValue("etat","feedback")){
                //setValue("desire",-0.7);
                //setValue("time","past");
                //setValue("cause","user");
                int v = Int32.Parse(getValue("faute").ToString());
                setValue("faute",v+1);
            }
            */
    

            //*******************Exercice4bis*************************

            /*
            if (hasValue("humeur","positive")){
                setValue("desire",0.7);

            }
            if (hasValue("humeur","negative")){
                setValue("desire",-0.7);
            }
            */
            if (hasValue("raison","activite")){
                setValue("cause","self");
                setValue("raison","ignore");
            }
            if (hasValue("raison","autre")){
                setValue("cause","other");
                setValue("raison","ignore");
            }   
            
            //***************Exercice 5****************
            //les modifications ont été ajoutés au code
            if (hasValue("etat","feedback")){
                double desire=0.3;
                if (hasValue("correct",true)){
                    int v = Int32.Parse(getValue("appri").ToString());
                    setValue("appri",v+1);
                    //setValue("correct","inconnue");
                }
                if (hasValue("correct",false)){
                    int v = Int32.Parse(getValue("faute").ToString());
                    setValue("faute",v+1);
                    //setValue("correct","inconnue");
                }
                if (hasValue("humeur","positive")){
                    int bonnesRep = Int32.Parse(getValue("appri").ToString());
                    int mauvaisesRep = Int32.Parse(getValue("faute").ToString());
                    double a =Constants.ParametreAgent;
                    desire=a*bonnesRep/(bonnesRep+mauvaisesRep)+(1-a);
                    Console.WriteLine("positive");
                }
      
                if (hasValue("humeur","negative")){
                    int bonnesRep = Int32.Parse(getValue("appri").ToString());
                    int mauvaisesRep = Int32.Parse(getValue("faute").ToString());
                    double a =Constants.ParametreAgent;
                    desire=a*bonnesRep/(bonnesRep+mauvaisesRep);
                    Console.WriteLine("negative "+desire );
                }
                setValue("desire",desire);
                setValue("time","past");

            }
            if (hasValue("etat","apprendre")){
                int bonnesRep = Int32.Parse(getValue("appri").ToString());
                if (bonnesRep>7){
                    setValue("desire",0);
                }
                else{
                    setValue("desire",0.3);
                }
                setValue("time","future");
            }

            if (hasValue("etat","reciter")){
                int n = Int32.Parse(getValue("appri").ToString());
                setValue("desire",0.5*(-n/7.0));
                setValue("time","future");
            }
        }
        public override (string,double) getCurrentEmotion(){
            double desire = Double.Parse(getValue("desire").ToString());
            //**************Exercice 3***********************
            /*
            if (hasValue("etat","feedback")&&hasValue("correct",true)){
                return ("joie",0.5);
            }
            if (hasValue("etat","feedback")&&hasValue("correct",false)){
                return ("tristesse",0.5);
            }
            else{
                return base.getCurrentEmotion();
            }
            */
            if (hasValue("time","past")){
                if (desire>0){
                    return ("joy",desire);
                } 
                if (desire <0){
                    if (hasValue("cause", "user")){
                        return ("anger",-desire);
                    }
                    if (hasValue("cause","self") || hasValue("cause","other")){
                        return ("distress",-desire);
                    }
                }
            }   
            if (hasValue("time","future")){
                if (desire <0){
                    return ("fear",-desire);
                }
                if (desire > 0){
                    return ("hope",desire);
                }
            }
            return ("neutral",0);                          
        }
        public override bool done() {
            return hasValue("etat","fin");
        }

        /* la méthode de lancement */
        static void Main(string[] args) {
            Dialogue d = readDialogueFile<Swahili>("swahili.json");
            d.run();
        }

    }
}