using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Functions
{
    public static class asciiFunction
    {
        public static string randomDraw()
        {
            string[] asciiDrawings = new string[]
                                {
                                            // Cars

                                            @"
                                   _______
                                 /|_||_\`.__
                                (   _    _ _\
                                =`-(_)--(_)-'", // Car 2 (Compact Side View)

                                            @"
                                     _______
                                   _//  __  \\_
                                  / |  /__\  | \
                                 '(_)--o--(_)--'", // Car 3 (Stylish Car)

                                            @"
                                     ______
                                  __//_||_\\_
                                 /_  _____  _\
                                  /_/     \_\\", // Car 4 (Boxy Style)

                                            // Animals
                                            @"
                                      /\_/\
                                     ( o.o )
                                      > ^ <", // Cat

                                            @"
                                   /^ ^\\
                                  / 0 0 \\
                                  V\ Y /V
                                   / - \\
                                  /    |
                                 V__) ||", // Dog

           


                                            @"
                                  ,__,
                                  (oo)____
                                  (__)    )\\
                                     ||--|| *", // Cow



                                            @"
                                   (o)(o)
                                  /     _\\
                                  \  '-' /
                                   '----'", // Frog Face

                                            @"
                                    ,  ,
                                   (o o)
                                  /'\'/'\
                                   """"", // Penguin

                                            @"
                                  ⊂(◉‿◉)つ",
                                                        @"
                                 (\_/)
                                 ( •_•) ♥
                                 / ", // Bunny Giving a Heart

                                            // 5. Little Robot Friend
                                            @"
                                  [ o_o ]
                                   <| |>
                                   /---\",
                                            @"
                                 (\_/) 🌸
                                 (•ᴥ•) 
                                  /  \",// Bunny Holding Flower

                                                        @"
                                 (\_/) 🌸
                                 (•ᴥ•) 
                                 / ", // Bunny Holding Flower

                                            // 2. Cat with Balloon
                                            @"
                                 /\_/\ 🎈
                                ( o.o ) 
                                 > ^ <", // Cat Holding Balloon

                                            // 3. Penguin with Ice Cream
                                            @"
                                  🐧 🍦
                                 ( '.' )
                                 /  V  \
                                (_( )_( )", // Penguin with Ice Cream

                                            // 4. Bear with Honey
                                            @"
                                 ʕ •ᴥ•ʔ 🍯
                                 /  ▌▌
                                  ∪∪", // Bear with Honey Pot

                                            // 5. Bird with Note
                                            @"
                                  🐦 🎶
                                  (o.o)
                                  /)  ) 
                                  ""--""",
                                               @"
                                  _______
                                 /       \
                                |  O   O  |
                                |    ^    |
                                 \  \_/  /
                                  \_____/", // Smiling Face
                                                 @"
                                   (\__/)
                                   ( •_•)
                                  / ", // Bunny Holding Books

                                @"
                                   (^.^)
                                   <)__)>✏️", // Little Mouse with Pencil


                                                        @"
                                   |\---/|
                                   | o_o |
                                    \_^_/", // Cat Face
                                                   @"
                                     (\__/)
                                     ( •_•)
                                    / >🌹", // Bunny Holding Flower
                                            @"
                                     /\_/\
                                    ( ^_^ )
                                     (  -  )", // Happy Cat Face

                                };

            // Randomly select an ASCII drawing
            Random random = new Random();
            int randomIndex = random.Next(asciiDrawings.Length);
            return asciiDrawings[randomIndex];

            // Output the selected ASCII drawing
        }

        public static string emojisDraw()
        {
            string[] emojis = new string[]
{
    @"
                      ( ͡° ͜ʖ ͡°)", // Lenny Face

    @"
                      (͠◉_◉᷅ )", // Intense Stare

    @"
                      (╯°□°）╯︵ ┻━┻", // Table Flip

    @"
                      ¯\_(ツ)_/¯", // Shrug

    @"
                      (ง •̀_•́)ง", // Fighting Spirit

    @"
                      (⌐■_■)", // Cool Sunglasses

    @"
                      (╥﹏╥)", // Crying Face

    @"
                      (¬‿¬)", // Smirking Face

    @"
                      (✿◠‿◠)", // Cute Smiling

    @"
                      (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧", // Excited Expression

    @"
                      ( ◕‿◕)", // Happy Face

    @"
                      ⊂(◉‿◉)つ", // Hug

    @"
                      (* ^ ω ^)", // Cheerful

    @"
                      (￣▽￣)/", // Waving

    @"
                      (¬‿¬ )", // Wink

    @"
                      ಠ_ಠ", // Disapproving Look

    @"
                      (｡♥‿♥｡)", // Love Blush

    @"
                      (ﾉ´ з `)ノ", // Sending Love

    @"
                      (✿ ♥‿♥)", // Cute Love

    @"
                      ( •_•) ( •_•)>⌐■-■ (⌐■_■)", // Sunglasses on

    @"
                      (｡•̀ᴗ-)✧", // Wink and Sparkle

    @"
                      ( ͡ᵔ ͜ʖ ͡ᵔ )", // Content Smile

    @"
                      (╬ Ò﹏Ó)", // Angry

    @"
                      (∩^o^)⊃━☆ﾟ.*･｡ﾟ", // Magic

    @"
                      (*≧ω≦)", // Happy Shy

    @"
                      (✧ω✧)", // Star Eyes

    @"
                      ʕ•ᴥ•ʔ", // Bear

    @"
                      ヽ(•‿•)ノ", // Celebratory

    @"
                      (^人^)", // Thankful

    @"
                      (｡•́︿•̀｡)", // Sad Face

    @"
                      (´･_･`)", // Worried

    @"
                      (>_<)", // Frustrated

    @"
                      (✿❛◡❛)", // Flower Smile

    @"
                      (/^-^(^ ^*)/", // High-Five

    @"
                      ( •_•)>⌐■-■", // Putting on Sunglasses

    @"
                      (✿◕‿◕✿)", // Flower Face

    @"
                      ┬─┬ ノ( ゜-゜ノ)", // Calm Table Put Back

    @"
                      ٩(◕‿◕｡)۶", // Excited Cheer

    @"
                      (´｡• ᵕ •｡`)", // Shy Smile

    @"
                      (•̀ᴗ•́)و ̑̑", // Motivated

    @"
                      ( ˘︹˘ )", // Unhappy Face

    @"
                      (♥ω♥*)", // In Love

    @"
                      ヽ(⌐■_■)ノ♪♬", // Dancing with Sunglasses

    @"
                      (╯︵╰,)", // Sad and Crying

    @"
                      (◕ᴗ◕✿)", // Happy Flower Girl

    @"
                      ٩(｡•́‿•̀｡)۶", // Excited Little Dance
};


            Random random = new Random();
            return emojis[random.Next(emojis.Length)];
        }


       public static string[] GetMovingObjectFrame()
        {
            return new string[]
{
    @"
     😊/ 💃
    /|   
   / \ 🎉
", // Frame 1: Right arm up, right leg forward, with dance emojis

    @"
     😊
     /|\   
    /  \ ✨
", // Frame 2: Both arms down, legs together, with smiling face and sparkle

    @"
    \ 🤩
       | \   
      / \ 🎶
", // Frame 3: Left arm up, left leg forward, with star-struck face and music note

    @"
        😊/ 🎵
        |\  
       /  \ 🕺
", // Frame 4: Right arm up, right leg stepping forward, with music note and dancing emoji

    @"
          😎
          /|\  
          / \ 🌟
", // Frame 5: Both arms down, balanced pose, with cool face and star

    @"
             🤩\ ✨
             | \  
            / \ 🌀
", // Frame 6: Left arm up, moving right with left leg forward, with sparkle and swirl

    @"
                😊/ 🎊
                |\  
               /  \ 🌈
" // Frame 7: Right arm up, stepping further to the right, with celebration and rainbow
};


        }


        public static string[] GetMovingCat()
        {
            return new string[]
            {
        @"
༼つಠ益ಠ༽つ                                             ((Σ≡=─🔥
", // Frame 1: Rocket far away

        @"
༼つಠ益ಠ༽つ                                    ((Σ≡=─🔥
", // Frame 2: Rocket getting closer

        @"
༼つಠ益ಠ༽つ                          ((Σ≡=─🔥
", // Frame 3: Rocket even closer

        @"
༼つಠ益ಠ༽つ                ((Σ≡=─🔥
", // Frame 4: Rocket much closer

        @"
༼つಠ益ಠ༽つ          ((Σ≡=─🔥
", // Frame 5: Rocket nearing the cat

        @"
༼つಠ益ಠ༽つ   ((Σ≡=─🔥
", // Frame 6: Rocket almost reaching the cat

        @"
༼つ^‿^༽つ 🎉🎊✨😺🎉🎊✨😺
", // Frame 7: The cat smiles with celebratory emojis
            };
        }



    }

}
