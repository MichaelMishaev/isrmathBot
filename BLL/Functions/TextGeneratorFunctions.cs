﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


    }
}