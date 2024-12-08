using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace BLL.Functions
{
    public static class TextGeneratorFunctions
    {
        public static string GetCorrectAnswerText()
        {
            var correctAnswers = new[]
{
    "🎉 כל הכבוד! תשובה נכונה! 😊",
    "מדהים! 🥳 ענית נכון! ✨",
    "נכון מאוד! 🌟 תשובה מצוינת! 👏",
    "😊 יפה מאוד! תשובה נכונה! 👍",
    "וואו, 🚀 הצלחת! תשובה מושלמת! 🔥",
    "👏 נהדר! ענית על הכל נכון! 🌠",
    "תשובה נכונה, 💪 כל הכבוד! 🏆",
    "🎊 כל הכבוד, עשית זאת נכון! ⭐",
    "💡 מעולה! תשובה נכונה! 🎉",
    "⭐ נהדר, הצלחת! תשובה נכונה! 👏",
    "פנטסטי! 🎈 תשובה נכונה! ✨",
    "🍎 נפלא! תשובה מדויקת! 😊",
    "וואו! 🔥 תשובה מצוינת! 🥳",
    "🎇 מדהים! ענית נכון! 💯",
    "✨ כל הכבוד! תשובה מושלמת! 🎉",
    "💪 יפה מאוד! בכיוון הנכון! 🚴‍♂️",
    "מעולה, 🎊 הצלחת! תשובה נכונה! 💯",
    "תשובה נכונה! 🚀 אתה/את תותח/תותחית! 🏆",
    "נהדר! 🎯 תשובה מדויקת ונכונה! 🌟",
    "🍀 כל הכבוד! תשובה נכונה! 👏",
    "🌈 איזה יופי! תשובה מצוינת! ✨",
    "🔥 מצוין! ענית נכון! 💪",
    "🎯 בול פגיעה! תשובה נכונה! 🎉",
    "ממש כוכב! 👑 תשובה מושלמת! 🌠",
    "🔄 המשכת לעשות את זה! תשובה נכונה! 👍",
    "😊 מעולה! תשובה מושלמת! 🌟",
    "🔥 תשובה נכונה שוב! כל הכבוד! 🎉",
    "💥 אתה/את פשוט אלוף/ה! 🏆",
    "✨ תשובה נכונה, 🚀 כל הכבוד לך!",
    "מבריק! ✨ תשובה מושלמת! 👏",
    "📏 תשובה מדויקת מאוד! 👍",
    "🌠 תשובה נכונה, המשכת לנסות! 💪",
    "איזה תותח/ית! 🎉 תשובה מדויקת! 🌟",
    "😄 מעולה! ענית נכון ובקלות! ✨",
    "🔥 פנטסטי! 🍾 ענית נכון!",
    "👏 תשובה נכונה! 🥇",
    "🎉 יפה! תשובה מצוינת! 😄",
    "פשוט מדהים! 🎆 🌟",
    "💙 עשית את זה נהדר! 👏",
    "✨ אין עליך! 🔝",
    "💚 תשובה נכונה! 👍",
    "את/ה פשוט מעולה! 🚀 🔥",
    "🌟 תשובה נהדרת, המשך כך! 😊",
    "🦸 ממש גיבור/ת היום! 🌈",
    "נפלא! 🌼 המשך כך! 💪",
    "נהדר! 🚀 פשוט פנטסטי! 🎊",
    "✨ תשובה נכונה ומהירה! ⏱️",
    "👍 מקסים! ענית מדויק! 🌸",
    "🔥 את/ה על הגל! 🏄‍♂️",
    "⭐ מצוין! תשובה חזקה! 💪",
    "✨ מדהים! אין מילים! 👏",
    "💯 תשובה מושלמת, כל הכבוד! 🎉",
    "שיחקת אותה! 🎯 🔥",
    "🚀 עלית על זה! 💪"
};
            var random = new Random();
            var randomCorrectAnswer = correctAnswers[random.Next(correctAnswers.Length)];
            return randomCorrectAnswer;

        }

        public static string GetMotivated()
        {
            var motivationalPhrases = new[]
{
    "💪 אבל קדימה, אסור לוותר! 🔥",
    "אבל בואו נעשה את זה, 💥 לא מוותרים! ✨",
    "⚡ אבל קדימה, ממשיכים בכל הכוח! 💪",
    "אל תוותרו! אפשר לעשות את זה! 🚀",
    "אבל קדימה, אנחנו ממשיכים קדימה! 😊💥",
    "💪 לא מוותרים, הולכים עד הסוף! ⭐",
    "אבל יאללה, נמשיך לנסות! 🎯🔥",
    "🔥 קדימה, אף פעם לא מוותרים! ✨",
    "אל תתייאשו, עוד קצת מאמץ! 💪🚀",
    "יאללה, אסור לוותר, ממשיכים! 💥😊",
    "✨ קדימה, יש לנו את זה! 💪",
    "🚀 אין דבר העומד בפני הרצון! קדימה! 🌟",
    "😊 יאללה, עוד קצת מאמץ ונצליח! 💪",
    "🔥 לא מוותרים, יש לנו כוח לעמוד בכל אתגר! 🚀",
    "💥 אל תוותרו! אנחנו חזקים ביחד! ✨",
    "אסור לוותר, הכוח אצלנו! 💪🔥",
    "יאללה, עוד קצת! אנחנו עושים את זה! 😊",
    "✨ נמשיך לנסות, לא מוותרים! 💥",
    "💪 תזכרו, אין דבר שלא ניתן להשיג! ✨",
    "אנחנו יכולים לעשות את זה, יאללה קדימה! 🚀",
    "💪 קדימה, עוד קצת מאמץ ונגיע לשם! 😊🔥",
    "🌟 לא מוותרים אף פעם, הכוח בידיים שלנו! 💪",
    "🚀 קדימה, לא עוצרים! 🔥",
    "💥 אל תוותרו! אתם יכולים לעשות את זה! 😊",
    "✨ נמשיך לנסות בכל הכוח! 💪",
    "קדימה, אנחנו מצליחים יחד! 😊🔥",
    "💪 יאללה, הולכים עד הסוף! 💥",
    "⚡ לא מוותרים, רק ממשיכים קדימה! ✨",
    "עוד קצת, לא לוותר! אנחנו עושים את זה! 🚀",
    "🔥 תזכרו שהכוח אצלנו, לא מוותרים אף פעם! 😊",
    "💪 אסור לוותר, אנחנו בדרך להצלחה! ✨",
    "קדימה! לא מוותרים, כולנו חזקים! 🌟🚀",
    "😊 יאללה, אנחנו מתקרבים, רק עוד מאמץ אחד! 💪",
    "💥 נמשיך לנסות, כי אנחנו לא עוצרים אף פעם! 🔥",
    "✨ קדימה, הכוח בידיים שלנו! 💪",
    "לא מוותרים, אנחנו נצליח יחד! 😊🚀",
    "💪 אל תתייאשו, אנחנו עושים את זה יחד! 💥",
    "🔥 יאללה, קדימה, הכוח אצלנו! 😊",
    "💥 אסור לעצור עכשיו, אנחנו בדרך להצלחה! ✨",
    "😊 קדימה, עוד קצת ונגיע ליעד! 🚀",
    "לא מוותרים, אנחנו פה לנצח! 💪🔥",
    "✨ תזכרו כמה אנחנו חזקים, ונמשיך! 💥",
    "💪 לא עוצרים עד ההצלחה! 🚀😊",
    "קדימה, אנחנו ביחד בזה, לא מוותרים אף פעם! 🌟",
    "💥 יאללה, קדימה, הכל אפשרי! 💪",
    "😊 נמשיך לנסות עד שנצליח, אסור לוותר! 🔥",
    "אסור לוותר! יש לנו את זה! 💥✨",
    "💪 קדימה! אנחנו יכולים לעשות את זה יחד! 🚀",
    "🔥 יאללה, עוד מאמץ קטן ונגיע ליעד! 😊✨",
    "✨ אין דבר שלא נוכל להשיג, קדימה! 💪",
    "💥 הכוח בידיים שלנו, לא עוצרים! 🌟😊"
};

            var random = new Random();

            return motivationalPhrases[random.Next(motivationalPhrases.Length)];

        }

        public static string GetRandomCongratulatoryMessage(int exercisesSolvedToday)
        {
            string[] congratulatoryMessages = new string[]
            {
        $@"
   כל הכבוד! 🎉 פתרת {exercisesSolvedToday} תרגילים היום!
   זה פשוט מדהים! 😊🚀
   המשיכו ככה ותגיעו רחוק! ✨💪
",
        $@"
   וואו! פתרת {exercisesSolvedToday} תרגילים היום! 👏
   זה פשוט יוצא מן הכלל! 🌟
   תמשיכו לעבוד קשה ותצליחו בענק! 🚀💯
",
        $@"
   תותח/ית! פתרת {exercisesSolvedToday} תרגילים היום! 🎉
   אין כמוכם! 💪✨
   המשיכו בדרך הזאת ותגיעו רחוק מאוד! 🚀🌠
",
        $@"
   מדהים! פתרת {exercisesSolvedToday} תרגילים היום! 🌟
   כל הכבוד על המאמץ וההתמדה שלכם! ✨🎊
   תמשיכו כך, הצלחה בדרך! 🚀😊
",
        $@"
   איזה אלופים! פתרת {exercisesSolvedToday} תרגילים היום! 🏆
   כל הכבוד על ההשקעה והעבודה הקשה! 💪🎉
   הדרך שלכם להצלחה כבר התחילה! ✨🚀
"
            };

            Random random = new Random();
            int index = random.Next(congratulatoryMessages.Length);
            return congratulatoryMessages[index];
        }

        public static string GetCcompletionMessages()
        {
            var completionMessages = new[]
{
            "נכון! סיימת את כל התרגילים! 🎉",
            "כל הכבוד! כל התרגילים הושלמו! 🥳",
            "פנטסטי! סיימת את כל התרגילים! 🚀",
            "עבודה נהדרת! סיימת את כל התרגילים! 🌟",
            "מדהים! סיימת בהצלחה את כל התרגילים! 😊",
            "עבודה מעולה! כל התרגילים הושלמו! 👏",
            "ברכות! סיימת את כל התרגילים! 🏆",
            "מצוין! עשית את זה! כל התרגילים הושלמו! 🎊",
            "נהדר! פתרת כל תרגיל ותרגיל! 💯",
            "מעולה! סיימת את כל התרגילים! ⭐",
            "יש! הצלחת לפתור את כל התרגילים! 🎈",
            "מרשים! עברת את כל התרגילים! 🎯",
            "נהדר! סיימת את כל התרגילים בהצלחה! 🚴‍♂️",
            "מעולה! כל התרגילים הושלמו בהצלחה! 💡",
            "פנומנלי! כל תרגיל הושלם! 🏅",
            "מאמץ פנטסטי! סיימת את כולם! 🥇",
            "כל הכבוד! עברת את כל התרגילים! 🌠",
            "עבודה נפלאה! כל התרגילים הושלמו בהצלחה! ✨",
            "מעולה! הצלחת בכל התרגילים! 💥",
            "מרשים! הצלחת בכל התרגילים! 🌈"
        };

            var random = new Random();
            var randomCompletionMessage = completionMessages[random.Next(completionMessages.Length)];

            return randomCompletionMessage;
        }

        public static string GetFormattedExerciseQuizMessage(string exercise, int remainingTime)
        {
            // Array of possible responses
            string[] exerciseMessages = new[]
            {
        $"⏱️**{remainingTime}** שניות! 🎯\n\n{exercise}",
        $"⏱️**{remainingTime}** שניות! 🚀\n\n{exercise}",
        $"⏱️**{remainingTime}** שניות! ✨\n\n{exercise}",
        $"⏱️**{remainingTime}** שניות! 🏆\n\n{exercise}",
        $"⏱️**{remainingTime}** שניות! 🔥\n\n{exercise}",
        $"⏱️**{remainingTime}** שניות! 📢\n\n{exercise}",
        $"⏱️**{remainingTime}** שניות! 🌟\n\n{exercise}",
        $"⏱️**{remainingTime}** שניות! 💡\n\n{exercise}",
        $"⏱️**{remainingTime}** שניות! 🎉\n\n{exercise}",
        $"⏱️**{remainingTime}** שניות! 🧠\n\n{exercise}"
    };

            // Choose one random message from the array
            Random random = new Random();
            string randomMessage = exerciseMessages[random.Next(exerciseMessages.Length)];

            // Return the chosen message
            return randomMessage;
        }


        public static string GetRandomExerciseMessage(string exercise)
        {
            // Array of possible responses
            string[] exerciseMessages = new[]
            {
        $"😃 כן! הגיע אתגר חדש! 🚀\n\n{exercise}\n\n✨ קדימה, תנסו את זה! 💥",
        $"✨ וואו, זה הולך להיות כיף! 🤩 בואו נפתור את זה יחד 📚\n\n{exercise}\n\n💪 בהצלחה! 🚀",
        $"💪 זה הזמן להראות מה אתם יודעים! 📢\n\n{exercise}\n\n📝 תנו את הכי טוב שלכם! 🌟",
        $"📢 קדימה! התרגיל הבא כבר כאן! ✨ תנסו ותצליחו 💥\n\n{exercise}\n\n😄 בהצלחה! 🎯",
        $"🧠 בואו נעשה תרגול חכם! 🚀 אני בטוח שתצליחו! ✏️\n\n{exercise}\n\n🍀 בהצלחה חברים! 😊",
        $"🚀 תרגיל חדש נחת! 🎉 בואו נראה את הכוח שלכם 💪\n\n{exercise}\n\n🌟 תנו בראש! 🔥",
        $"🎉 התרגיל הבא ממתין לכם! ✨ קדימה לתשובה נכונה 💯\n\n{exercise}\n\n🔢 בהצלחה רבה! 📖"
    };

            // Choose one random message from the array
            Random random = new Random();
            string randomExerciseMessage = exerciseMessages[random.Next(exerciseMessages.Length)];

            // Return the chosen message
            return randomExerciseMessage;
        }


        public static string GetQuizStartMessage()
        {
            string[] quizStartMessages = new[]
           {
    "🎉 **חידון התחיל עכשיו!** 🕐\n⏱️ יש לכם **60 שניות** לענות על כמה שיותר שאלות! 🚀",

    "🚨 **זה חידון!** ⏱️\nיש לכם **60 שניות** בלבד להצליח בגדול! 💥",

    "🏆 **החידון התחיל!** 🕒\n⏱️ **60 שניות** של שאלות ואתם על השעון! ⏳",

    "⚡ **חידון בזק!** ⏱️\nיש לכם **60 שניות** להראות מה אתם יודעים! 🎯",

    "🚀 **היכון, חידון, צא!** 🕐\n⏱️ **60 שניות** לענות על כמה שיותר שאלות! 💪"
};




            Random random = new Random();
            return quizStartMessages[random.Next(quizStartMessages.Length)];

        }

        public static string GetRandomCorrectOrIncorrectMessage(bool isCorrect)
        {
            string[] messages;

            if (isCorrect)
            {
                messages = new[]
                {
        "✅ תשובה נכונה! 💥",
        "🎉 מעולה! 🌟",
        "🚀 יפה מאוד! 🥇",
        "💪 כל הכבוד! 💯",
        "🔥 נכון מאוד! 😊",
        "🎯 בול פגיעה! 🏆",
        "💎 מושלם! 🌹",
        "✨ שיחקתם אותה! 💪",
        "🌟 מצוין! 🎖️",
        "🥳 אליפות! 🌈"
    };
            }
            else
            {
                messages = new[]
                {
        "❌ לא נכון! ✨",
        "😅 נסו שוב! 🚀",
        "🌟 כמעט, אבל לא. 💡",
        "❓ טעות קטנה. 🍀",
        "🙃 קרוב, אבל לא. 🎯",
        "😓 לא הפעם! 🌈",
        "😐 טעות, נמשיך! 💥",
        "🙁 לא נכון, קדימה! 🚀",
        "😕 שגוי, תנסו שוב! 🔥",
        "🤔 נסו שוב, זה יצליח! 🧠"
    };
            }


            Random random = new Random();
            return messages[random.Next(messages.Length)];
        }



        public static string Get5ExerciseSolvedMessage(string studentName)
        {
            string[] congratulatoryMessages = [
    $"🎉✨ כל הכבוד {studentName}! פתרת עוד 5 תרגילים! שיא חדש! 🏆✨",
        $"🥳💡 איזה יופי {studentName}! 5 תרגילים נוספים מאחוריך! 👏📚",
        $"🔥🏆 וואו {studentName}! פתרת עוד 5 תרגילים! אין כמוך! ✏️💪",
        $"🎯✨ מדהים {studentName}! 5 תרגילים הושלמו בהצלחה! איזה הישג! 🎉📖",
        $"🎉🥳 כל הכבוד {studentName}! הצלחת לפתור 5 תרגילים! פשוט מדהים! 🏆✨",
        $"📚🚀 איזה הישג {studentName}! 5 תרגילים נפתרו! עבודה נהדרת! 💡👏",
        $"🌟🔥 מצוין {studentName}! סיימת 5 תרגילים בצורה מושלמת! 🏆✏️",
        $"💃🕺 מדהים {studentName}! 5 תרגילים מאחוריך! איזה הישג נפלא! 🎯📖",
        $"🎉🔥 כל הכבוד {studentName}! הצלחת לפתור 5 תרגילים נוספים! שיא חדש בדרך! 🏆📚",
        $"🥳🚀 וואו {studentName}! 5 תרגילים מאחוריך! איזה הישג נפלא! ✏️💡",
        $"🔥✨ איזה יופי {studentName}! פתרת 5 תרגילים בצורה מושלמת!  מעולה! 🎉📖",
        $"🌟📚 מדהים {studentName}! 5 תרגילים נוספים מאחוריך! פשוט אין עליך! 🏆💪",
        $"✨🎯 נהדר {studentName}! פתרת עוד 5 תרגילים! אליפות! 🎉📚",
        $"💪🔥 יופי {studentName}! 5 תרגילים נפתרו בהצלחה! איזה הישג! 🏆✨",
        $"🎉✨ כל הכבוד {studentName}! פתרת 5 תרגילים! 🚀📖",
        $"🥳💡 איזה יופי {studentName}! 5 תרגילים נוספים נפתרו! עבודה נהדרת! 🏆🔥",
        $"🔥🚀 מדהים {studentName}! הצלחת לפתור 5 תרגילים בצורה מושלמת! כל הכבוד! 🌟📚",
        $"✨📖 וואו {studentName}! פתרת 5 תרגילים נוספים! פשוט נדיר! 🏆💪",
        $"🎉🥳 נהדר {studentName}! הצלחת לפתור 5 תרגילים! ! 🌟✏️",
        $"📚🚀 איזה הישג {studentName}! סיימת 5 תרגילים נוספים! עבודה נהדרת! ✨🔥",
        $"🌟🎯 מדהים {studentName}! פתרת 5 תרגילים! המשך/י כך! 🏆📖",
        $"🔥✨ כל הכבוד {studentName}! הצלחת לפתור 5 תרגילים נוספים! פשוט אין כמוך! 🥳📚",
        $"✨🎉 וואו {studentName}! פתרת 5 תרגילים בצורה מושלמת! שיא חדש! 🎯🚀",
        $"📖🔥 כל הכבוד {studentName}! 5 תרגילים נוספים מאחוריך! איזה הישג מרשים! 🏆💪"

];

            // Pick a random message
            Random random = new Random();
            string randomExerciseMessage = congratulatoryMessages[random.Next(congratulatoryMessages.Length)];

            return randomExerciseMessage;

        }

        public static string GetFinishedExercisesMessage()
        {
            string[] messages = new string[]
   {
            "🎉 וואו! אתם אלופים! 👑 סיימתם הכל! 💪✨ כל הכבוד! 🏆😄\n✨ בקרוב יהיו עוד תרגילים כיפיים! 📚🤩",
            "🚀 כל הכבוד! 🏅 אתם פשוט תותחים! 💥 המשיכו ככה! 😊",
            "🎊 איזה הישג! 🥳 כל הכבוד על המאמצים שלכם! 💪 תרגילים חדשים בדרך! 📖🤓",
            "👏 מדהים! 🌟 סיימתם הכל בצורה מעולה! 🥇 המשיכו להיות כוכבים! ✨",
            "💫 וואו, אתם פשוט מצוינים! 🏆 כיף לראות אתכם מסיימים הכל בהצלחה! 😄",
            "🔥 אתם פשוט אלופים! 🏅 סיימתם את התרגילים בצורה מרשימה! 💪 המשיכו ככה! 🚀",
            "🌟 כל הכבוד! 🏆 אתם מלכי התרגילים! 👑 מחכים לכם תרגילים נוספים בקרוב! 📚😊",
            "✨ איזה ביצוע מרשים! 🤩 אתם מדהימים! 😍 תמשיכו להצליח בתרגילים הבאים! 📖💡",
            "🎉 הצלחה מרשימה! 🥳 אתם פשוט מדהימים! 💥 מחכים לכם אתגרים חדשים! 💪🚀",
            "💪 כל הכבוד על ההשקעה! 🏆 אתם פשוט נפלאים! 💛 בקרוב יהיו עוד תרגילים כיפיים! 😊"
   };

            Random random = new Random();
            int index = random.Next(messages.Length);

            return messages[index];
        }

        public static string GetLevelUpdateMessage(string newLevel, string changeType)
        {
            // Messages for downgrade to Easy
            var downgradeToEasyMessages = new[]
            {
        "🌟 **אל דאגה 😊! התרגילים יהיו פשוטים יותר עכשיו.** 🚀✨ **אנחנו מאמינים בכם! ממשיכים להתקדם!** 📘🌈",
        "✨ **לפעמים כדאי לקחת צעד אחורה 😊🎉. הרמה עכשיו קלה יותר כדי לחזור בביטחון!** 🚀📘🌟",
        "🌈 **התאמנו את הרמה 🎉 כדי להקל עליכם 😊✨! קדימה, לא עוצרים!** 🚀🌟📘"
    };

            // Messages for upgrade to Hard
            var upgradeToHardMessages = new[]
            {
        "🚀 **ברכות על הקידום 🎉! אתם עכשיו בדרגת גאונים 🤓✨!** 🌈 **קדימה, נראה את הגאונות שלכם!** 🌟📘✨",
        "🌟 **WOW! אתם משיגים גבהים חדשים! התרגילים ברמה הגבוהה ביותר עכשיו 🚀✨!** 🎉 **אתם מדהימים!** 🌈📘",
        "🎉 **מדהים! רמת גאונות מחכה לכם עכשיו 🚀🌟!** 🌈 **קדימה, להמשיך עם כל הכוח!** 📘✨"
    };

            // Messages for change to Medium
            var mediumUpgradeMessages = new[]
            {
        "🎉 **התקדמות נהדרת! מעכשיו תרגילים ברמה בינונית מחכים לכם 🚀✨!** 🌟 **אתם בדרך הנכונה!** 📘🌈",
        "✨ **ברכות על ההתקדמות 🎉! התרגילים ברמה מאוזנת יותר עכשיו 🌟!** 🚀 **ממשיכים להוכיח את היכולות!** 🌈📘",
        "📘 **שדרוג קטן ברמה 🚀✨!** 🌈 **עכשיו יהיו צרגילים טיפה קשים יות!** 🌟🎉"
    };

            var mediumDowngradeMessages = new[]
            {
        "🌟 **לא נורא 😊! התרגילים עכשיו ברמה בינונית.** 🚀✨ **זה הזמן להחזיר את הביטחון!** 🎉📘🌈",
        "✨ **התאמנו את הרמה כדי להקל 😊🎉. התרגילים עכשיו מאוזנים יותר!** 🚀 **קדימה, בואו נתקדם יחד!** 🌟📘",
        "🌈 **תרגילים ברמה בינונית לפניכם 😊✨!** 🚀 **לאט לאט, חוזרים לשגרה!** 🎉🌟📘"
    };

            // Select messages based on level and change type
            string[] selectedMessages;

            switch (newLevel)
            {
                case "Easy":
                    selectedMessages = downgradeToEasyMessages; // Only downgrade to Easy
                    break;
                case "Medium":
                    selectedMessages = changeType == "Upgraded" ? mediumUpgradeMessages : mediumDowngradeMessages;
                    break;
                case "Hard":
                    selectedMessages = upgradeToHardMessages; // Only upgrade to Hard
                    break;
                default:
                    selectedMessages = new[] { "🎈 **רמה חדשה הותאמה עבורכם! קדימה, נמשיך יחד!** 🚀✨📘" };
                    break;
            }

            // Pick a random message from the selected array
            var random = new Random();
            return selectedMessages[random.Next(selectedMessages.Length)];
        }


        public static string GetExerciseCreationText()
        {
            return @"התרגילים נמחקו. אנא ספק הוראות חדשות ליצירת תרגילים.
ליצירת תרגיל חדש כתוב בפורמט הבא:
ראשית: ***
לאחר מכן, בשורה חדשה, כתוב את ההוראה. לדוגמה:
צור מכפלות ב-5, מקסימום כפול 10
לאחר מכן תוכל להוסיף //
לדוגמה, כך:
//
5*5, 5*1

דוגמה מלאה:
***
צור מכפלות ב-5, מקסימום כפול 10
// 
5*5, 5*1";

        }




    }
}
