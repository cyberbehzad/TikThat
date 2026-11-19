using System;
using System.Collections.Generic;
using System.Linq;
using TikThat.Models;

namespace TikThat.Services;

public class AchievementUnlockResult
{
    public List<Achievement> NewlyDiscoveredAchievements { get; set; } = [];
    public List<Achievement> NewlyUnlockedAchievements { get; set; } = [];
    public int GainedPoints { get; set; }
}

public class AchievementService
{
    public IReadOnlyList<Achievement> GetAllAchievements()
    {
        return
        [
            // ==========================================
            // --- General Achievements (Visible) ---
            // ==========================================
            new() {
                Id = "tasks_10",
                Name = "تازه کار متعهد",
                Description = "انجام ۱۰ تسک در برنامه",
                Icon = "badge_bronze.png",
                Guide = "کافیست ۱۰ وظیفه مختلف را در برنامه با موفقیت تیک بزنید.",
                SampleTaskText = "اولین تسک من را کامل کن"
            },
            new() {
                Id = "tasks_50",
                Name = "نیمه حرفه ای",
                Description = "انجام ۵۰ تسک",
                Icon = "badge_silver.png",
                Guide = "با انجام ۵۰ وظیفه، نشان دهید که در برنامه ریزی جدی هستید.",
                SampleTaskText = "برنامه ریزی برای کارهای هفتگی"
            },
            new() {
                Id = "tasks_100",
                Name = "حرفه ای تمام عیار",
                Description = "انجام ۱۰۰ تسک",
                Icon = "badge_gold.png",
                Guide = "با انجام ۱۰۰ وظیفه به بالاترین سطح تعهد شخصی برسید.",
                SampleTaskText = "تکمیل پروژه کاری مهم"
            },
            new() {
                Id = "tasks_diamond",
                Name = "اسطوره بهره وری",
                Description = "انجام ۲۰۰ تسک و حفظ استریک مداوم",
                Icon = "badge_diamond.png",
                Guide = "تداوم در برنامه ریزی و انجام ۲۰۰ تسک مختلف.",
                SampleTaskText = "بررسی اهداف بلندمدت"
            },
            new() {
                Id = "streak_7",
                Name = "زنجیره هفتگی",
                Description = "حفظ ۷ روز مداوم استریک",
                Icon = "badge_streak7.png",
                Guide = "یک هفته کامل هر روز حداقل یک تسک را تیک بزنید.",
                SampleTaskText = "مرور هفتگی زنجیره فعالیت"
            },
            new() {
                Id = "streak_30",
                Name = "تعهد یک ماهه",
                Description = "حفظ ۳۰ روز استریک متوالی",
                Icon = "badge_streak30.png",
                Guide = "یک ماه کامل زنجیره روزانه برنامه ریزی خود را زنده نگه دارید.",
                SampleTaskText = "جشن تکمیل یک ماه تعهد مداوم"
            },
            new() {
                Id = "streak_100",
                Name = "اراده پولادین",
                Description = "ثبت ۱۰۰ روز استریک مداوم",
                Icon = "badge_streak100.png",
                Guide = "رسیدن به مرز ۱۰۰ روز متوالی تلاش و پشتکار شخصی.",
                SampleTaskText = "ثبت صدمین روز متوالی پشتکار"
            },
            new() {
                Id = "geo_explorer",
                Name = "میدانی و چابک",
                Description = "تکمیل تسک دارای موقعیت مکانی",
                Icon = "badge_geo.png",
                Guide = "وظیفه ای که برای آن مکان نقشه تنظیم شده را در محل انجام دهید.",
                SampleTaskText = "خرید حضوری با یادآور موقعیت"
            },
            new() {
                Id = "bio_vault",
                Name = "گاوصندوق امنیتی",
                Description = "مدیریت تسک با قفل بیومتریک",
                Icon = "lock_icon2.png",
                Guide = "یک تسک قفل شده شخصی را باز کرده و تکمیل کنید.",
                SampleTaskText = "ثبت گزارش محرمانه و قفل آن"
            },
            new() {
                Id = "type_reading",
                Name = "کتاب خوان",
                Description = "تکمیل تسک مطالعه و یادگیری",
                Icon = "badge_reading.png",
                Guide = "تسکی که شامل کلمه «مطالعه» یا «کتاب» باشد را کامل کنید.",
                SampleTaskText = "مطالعه کتاب به مدت ۲۰ دقیقه"
            },
            new() {
                Id = "type_workout",
                Name = "ورزشکار",
                Description = "تکمیل تسک های ورزشی",
                Icon = "badge_workout.png",
                Guide = "تسکی که شامل کلمات «ورزش»، «تمرین» یا «باشگاه» باشد را کامل کنید.",
                SampleTaskText = "انجام تمرین ورزشی روزانه"
            },
            new() {
                Id = "type_coding",
                Name = "کدنویس خستگی ناپذیر",
                Description = "توسعه و برنامه نویسی",
                Icon = "badge_coding.png",
                Guide = "تسکی شامل کلمات «کد»، «برنامه نویسی» یا «دیباگ» را به پایان برسانید.",
                SampleTaskText = "کدنویسی بخش جدید نرم افزار"
            },
            new() {
                Id = "type_selfcare",
                Name = "مراقبت از خود",
                Description = "رسیدگی به سلامت روان و استراحت",
                Icon = "badge_selfcare.png",
                Guide = "تسکی برای مراقبت از سلامت فردی و آرامش انجام دهید.",
                SampleTaskText = "مدیتیشن و استراحت کوتاه"
            },
            new() {
                Id = "weekly_planner",
                Name = "برنامه ریز آینده نگر",
                Description = "ثبت تسک برای روزهای آینده",
                Icon = "badge_weekly_planner.png",
                Guide = "برای روزهای بعد تسکی را در تقویم زمان بندی و تیک بزنید.",
                SampleTaskText = "برنامه ریزی تسک های شنبه آینده"
            },
            new() {
                Id = "daily_clear",
                Name = "روز تمیز",
                Description = "تکمیل تمام تسک های یک روز",
                Icon = "badge_daily_clear.png",
                Guide = "در پایان روز تمام وظایف فعال خود را با موفقیت ببندید.",
                SampleTaskText = "بررسی و تیک زدن کارهای روزانه"
            },
            new() {
                Id = "fresh_start",
                Name = "شروع دوباره",
                Description = "شروع مجدد برنامه ریزی بعد از وقفه",
                Icon = "badge_fresh_start.png",
                Guide = "آغاز یک زنجیره تازه بعد از وقفه با یک تسک جدید.",
                SampleTaskText = "آغاز فعالیت مجدد با تسک جدید"
            },
            new() {
                Id = "ninja_focus_first",
                Name = "تمرکز نینجا",
                Description = "تکمیل اولین ماموریت نینجا",
                Icon = "ninja_icon2.png",
                Guide = "یک تسک را با تایمر نینجا مود قبل از پایان زمان تمام کنید.",
                SampleTaskText = "تمرکز عمیق روی تسک مهم"
            },

            // ==========================================
            // --- Secret Achievements ---
            // ==========================================
            new() {
                Id = "secret_nightowl",
                Name = "شب بیداری",
                Description = "انجام تسک بین ۲ تا ۴ صبح",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمه «نیمه شب» یا در بازه ۲ تا ۴ بامداد ثبت و تیک بزنید.",
                SampleTaskText = "مرور تسک در نیمه شب",
                IsSecret = true
            },
            new() {
                Id = "secret_first_fail",
                Name = "درس از شکست",
                Description = "تلاش مجدد در ماموریت نینجا",
                Icon = "secret_icon.png",
                Guide = "پس از اتمام زمان تایمر نینجا، دوباره تلاش کنید.",
                SampleTaskText = "تلاش مجدد در حالت نینجا",
                IsSecret = true
            },
            new() {
                Id = "secret_welcome_back",
                Name = "بازگشت قهرمان",
                Description = "رسیدگی به وظایف معوقه",
                Icon = "secret_icon.png",
                Guide = "تسکی با موضوع وظایف گذشته را تیک بزنید.",
                SampleTaskText = "بررسی وظایف معوقه",
                IsSecret = true
            },
            new() {
                Id = "secret_zen",
                Name = "آرامش ذن",
                Description = "تکمیل تسک با تمرکز و بدون تغییر وضعیت",
                Icon = "secret_icon.png",
                Guide = "انجام آرام و متمرکز تسک بدون ویرایش های مکرر.",
                SampleTaskText = "انجام آرام و با تمرکز تسک",
                IsSecret = true
            },
            new() {
                Id = "secret_night_gamer",
                Name = "گیمر شبانه",
                Description = "تکمیل تسک بازی در دل شب",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمه «گیم» یا «بازی» را در ساعات ۱ تا ۴ بامداد تیک بزنید.",
                SampleTaskText = "گیم زدن با رفقا ساعت یک بامداد",
                IsSecret = true
            },
            new() {
                Id = "secret_early_bird",
                Name = "سحرخیز قهرمان",
                Description = "تکمیل تسک سحرخیزی بین ۵ تا ۷ صبح",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمه «بیدارباش» یا «سحرخیز» را صبح زود تیک بزنید.",
                SampleTaskText = "بیدارباش و آغاز روز پرانرژی",
                IsSecret = true
            },
            new() {
                Id = "secret_pizza_lover",
                Name = "عاشق پیتزا",
                Description = "انجام تسک مرتبط با پیتزا",
                Icon = "secret_icon.png",
                Guide = "تسکی که کلمه «پیتزا» در عنوان آن باشد را تیک بزنید.",
                SampleTaskText = "سفارش یک پیتزای خوشمزه برای ناهار",
                IsSecret = true
            },
            new() {
                Id = "secret_bug_hunter",
                Name = "شکارچی باگ",
                Description = "برطرف کردن خطای نرم افزاری",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمات «باگ» یا «دیباگ» را به پایان برسانید.",
                SampleTaskText = "بررسی و حل باگ پروژه",
                IsSecret = true
            },
            new() {
                Id = "secret_coffee_addict",
                Name = "سوپر قهوه خور",
                Description = "نوشیدن یک فنجان قهوه پرانرژی",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمه «قهوه» یا «اسپرسو» را کامل کنید.",
                SampleTaskText = "نوشیدن قهوه برای انرژی بیشتر",
                IsSecret = true
            },
            new() {
                Id = "secret_wanderlust",
                Name = "سفر ماجراجویانه",
                Description = "برنامه ریزی برای سفر و گردشگری",
                Icon = "secret_icon.png",
                Guide = "تسکی با کلمات «سفر»، «مسافرت» یا «بلیط» را ثبت و تیک بزنید.",
                SampleTaskText = "برنامه ریزی مسافرت آخر هفته",
                IsSecret = true
            },
            new() {
                Id = "secret_music_vibes",
                Name = "عشق موسیقی",
                Description = "گوش دادن به قطعات موسیقی دلخواه",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمات «موزیک»، «آهنگ» یا «موسیقی» را به پایان برسانید.",
                SampleTaskText = "گوش دادن به آلبوم موسیقی جدید",
                IsSecret = true
            },
            new() {
                Id = "secret_mr_fixit",
                Name = "آچار فرانسه",
                Description = "تعمیر یا سر و سامان دادن به وسایل",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمه «تعمیر» را انجام دهید.",
                SampleTaskText = "تعمیر و رسیدگی به وسایل منزل",
                IsSecret = true
            },
            new() {
                Id = "secret_linux_nerd",
                Name = "دانشمند لینوکس",
                Description = "کار در خط فرمان لینوکس",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمات «لینوکس» یا «ترمینال» را تکمیل کنید.",
                SampleTaskText = "کانفیگ سرور لینوکس در ترمینال",
                IsSecret = true
            },
            new() {
                Id = "secret_master_chef",
                Name = "سرآشپز حرفه ای",
                Description = "طبخ یک وعده غذای تازه خانگی",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمه «آشپزی» را کامل کنید.",
                SampleTaskText = "آشپزی یک شام خوش طعم خانگی",
                IsSecret = true
            },
            new() {
                Id = "secret_money_saver",
                Name = "پس انداز هوشمند",
                Description = "کنار گذاشتن پس انداز و مدیریت هزینه ها",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمه «پس انداز» را تیک بزنید.",
                SampleTaskText = "واریز مبلغ ماهانه به پس انداز",
                IsSecret = true
            },
            new() {
                Id = "secret_shopaholic",
                Name = "خرید درمانی",
                Description = "انجام خریدهای برنامه ریزی شده",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمه «خرید» را انجام دهید.",
                SampleTaskText = "خرید وسایل مورد نیاز هفته",
                IsSecret = true
            },
            new() {
                Id = "secret_deep_clean",
                Name = "نظافت انفجاری",
                Description = "تمیزکاری و مرتب سازی محیط کار و زندگی",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمات «نظافت» یا «تمیزکاری» را ببندید.",
                SampleTaskText = "تمیزکاری و مرتب کردن میز کار",
                IsSecret = true
            },
            new() {
                Id = "secret_social_butterfly",
                Name = "دیدار با رفقا",
                Description = "دیدار و برقراری ارتباط با دوستان",
                Icon = "secret_icon.png",
                Guide = "تسکی با کلمات «دیدار» یا «دوستان» را ثبت و کامل کنید.",
                SampleTaskText = "دیدار و صحبت با دوستان قدیمی",
                IsSecret = true
            },
            new() {
                Id = "secret_polyglot",
                Name = "تسلط بر زبان",
                Description = "تمرین کلمات و یادگیری زبان جدید",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمات «زبان» یا «انگلیسی» را کامل کنید.",
                SampleTaskText = "تمرین لغات جدید زبان انگلیسی",
                IsSecret = true
            },
            new() {
                Id = "secret_stay_hydrated",
                Name = "سیراب و شاداب",
                Description = "نوشیدن آب کافی در طول روز",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل عبارت «نوشیدن آب» را تیک بزنید.",
                SampleTaskText = "نوشیدن ۸ لیوان آب گوارا",
                IsSecret = true
            },
            new() {
                Id = "secret_finance_guru",
                Name = "حساب و کتاب",
                Description = "رسیدگی به تراکنش ها و صورتحساب ها",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمات «فاکتور» یا «حسابداری» را علامت بزنید.",
                SampleTaskText = "رسیدگی به فاکتورها و مسائل مالی",
                IsSecret = true
            },
            new() {
                Id = "secret_meditation",
                Name = "آرامش درون",
                Description = "تسکین ذهن با مدیتیشن و خلوت ذهنی",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمه «مدیتیشن» را تکمیل کنید.",
                SampleTaskText = "مدیتیشن و ریلکس کردن ذهن",
                IsSecret = true
            },
            new() {
                Id = "secret_nature_walker",
                Name = "طبیعت گرد",
                Description = "قدم زدن در هوای آزاد و بوستان ها",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمات «پیاده روی» یا «طبیعت» را تیک بزنید.",
                SampleTaskText = "پیاده روی عصرگاهی در پارک",
                IsSecret = true
            },
            new() {
                Id = "secret_power_nap",
                Name = "چرت نیرو بخش",
                Description = "استراحت کوتاه نیمروزی",
                Icon = "secret_icon.png",
                Guide = "تسکی با کلمه «چرت» را انجام دهید.",
                SampleTaskText = "چرت کوتاه نیمروزی برای انرژی دوباره",
                IsSecret = true
            },
            new() {
                Id = "secret_movie_night",
                Name = "سینمای خانگی",
                Description = "تماشای اثر سینمایی یا سریال",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمات «فیلم» یا «سینما» یا «سریال» را انجام دهید.",
                SampleTaskText = "تماشای فیلم سینمایی مورد علاقه",
                IsSecret = true
            },
            new() {
                Id = "secret_podcast_time",
                Name = "شنونده دانا",
                Description = "گوش فرا دادن به اپیزود پادکست",
                Icon = "secret_icon.png",
                Guide = "تسکی با کلمه «پادکست» را به پایان برسانید.",
                SampleTaskText = "گوش دادن به قسمت جدید پادکست",
                IsSecret = true
            },
            new() {
                Id = "secret_ai_master",
                Name = "جادوگر هوش مصنوعی",
                Description = "توسعه و حل تسک با ابزارهای هوش مصنوعی",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل عبارت «هوش مصنوعی» یا «پرامپت» را تیک بزنید.",
                SampleTaskText = "پرامپت نویسی و کار با هوش مصنوعی",
                IsSecret = true
            },
            new() {
                Id = "secret_gift_self",
                Name = "هدیه به خود",
                Description = "پاداش دادن به خود بعد از تلاش",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمه «هدیه» یا «کادو» یا «جایزه» را انجام دهید.",
                SampleTaskText = "تهیه یک هدیه برای پاداش به خودم",
                IsSecret = true
            },
            new() {
                Id = "secret_digital_detox",
                Name = "خداحافظ دنیای مجازی",
                Description = "فاصله گرفتن موقت از صفحات نمایشگر",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل «سم زدایی» یا «دوری از گوشی» را ببندید.",
                SampleTaskText = "دو ساعت دوری کامل از گوشی و آفلاین بودن",
                IsSecret = true
            },
            new() {
                Id = "secret_friday_peace",
                Name = "آرامش آخر هفته",
                Description = "تکمیل موفق تسک های جمعه",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمه «جمعه» را در روز جمعه تکمیل نمایید.",
                SampleTaskText = "لذت بردن از کارهای آرام جمعه",
                IsSecret = true
            },
            new() {
                Id = "secret_speed_runner",
                Name = "سرعت عمل بالا",
                Description = "انجام تسک فوری در زمان کوتاه",
                Icon = "secret_icon.png",
                Guide = "تسکی با عبارت «فوری» را ثبت کرده و بلافاصله کامل کنید.",
                SampleTaskText = "انجام سریع و فوری یک کار عقب افتاده",
                IsSecret = true
            },
            new() {
                Id = "secret_midnight_tea",
                Name = "چای شبانه",
                Description = "نوشیدن چای آرامش بخش در دل شب",
                Icon = "secret_icon.png",
                Guide = "تسکی با موضوع چای یا دمنوش را در ساعات پایانی شب تیک بزنید.",
                SampleTaskText = "دم کردن چای تازه دم نیمه شب",
                IsSecret = true
            },
            new() {
                Id = "secret_clean_desk",
                Name = "ایستگاه کاری تمیز",
                Description = "سر و سامان دادن به میز کار و اتاق",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل تمیزکاری میز یا محیط کار را انجام دهید.",
                SampleTaskText = "مرتب سازی و گردگیری میز کار",
                IsSecret = true
            },
            new() {
                Id = "secret_git_commit",
                Name = "کامیت طلایی",
                Description = "پوش کردن کدهای توسعه نرم افزار",
                Icon = "secret_icon.png",
                Guide = "تسکی با موضوع کامیت یا گیت را علامت بزنید.",
                SampleTaskText = "کامیت و پوش کردن کدهای پروژه",
                IsSecret = true
            },
            new() {
                Id = "secret_family_first",
                Name = "اولویت خانواده",
                Description = "تماس یا وقت گذراندن با خانواده",
                Icon = "secret_icon.png",
                Guide = "تسکی با محوریت تماس یا دیدار خانواده ثبت کنید.",
                SampleTaskText = "تماس تلفنی و احوالپرسی با خانواده",
                IsSecret = true
            },
            new() {
                Id = "secret_doc_reader",
                Name = "مرجع خوان",
                Description = "مطالعه مستندات فنی و مقالات مرجع",
                Icon = "secret_icon.png",
                Guide = "تسکی شامل کلمه داکیومنت یا مستندات را کامل کنید.",
                SampleTaskText = "مطالعه داکیومنت رسمی پروژه",
                IsSecret = true
            },
            new() {
                Id = "secret_water_plant",
                Name = "باغبان مهربان",
                Description = "رسیدگی و آبیاری گیاهان",
                Icon = "secret_icon.png",
                Guide = "تسکی برای آب دادن به گلدان ها ثبت و کامل کنید.",
                SampleTaskText = "آب دادن به گلدان های اتاق",
                IsSecret = true
            },
            new() {
                Id = "secret_backup_hero",
                Name = "نجات بخش داده ها",
                Description = "تهیه فایل پشتیبان از سیستم",
                Icon = "secret_icon.png",
                Guide = "تسکی حاوی بکاپ یا پشتیبان گیری را انجام دهید.",
                SampleTaskText = "گرفتن نسخه پشتیبان از فایل ها",
                IsSecret = true
            }
        ];
    }

    private static string NormalizeText(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        return input.Trim()
                    .ToLowerInvariant()
                    .Replace("ي", "ی")
                    .Replace("ك", "ک")
                    .Replace("‌", " ")
                    .Replace("\u200c", " ")
                    .Replace("\u200b", "")
                    .Replace("\ufeff", "");
    }

    public AchievementUnlockResult CheckDiscoveryOnTaskCreated(string taskText, UserProfile profile)
    {
        var result = new AchievementUnlockResult();
        if (string.IsNullOrWhiteSpace(taskText)) return result;

        profile.DiscoveredSecretAchievementIds ??= [];
        profile.UnlockedAchievementIds ??= [];

        var text = NormalizeText(taskText);
        var nowTime = DateTime.Now.TimeOfDay;
        var todayDay = DateTime.Today.DayOfWeek;

        var secretAchievements = GetAllAchievements().Where(a => a.IsSecret);

        foreach (var ach in secretAchievements)
        {
            if (profile.UnlockedAchievementIds.Contains(ach.Id) ||
                profile.DiscoveredSecretAchievementIds.Contains(ach.Id))
                continue;

            bool isDiscovered = CheckSecretRuleMatch(ach.Id, ach.SampleTaskText, text, nowTime, todayDay);

            if (isDiscovered)
            {
                profile.DiscoveredSecretAchievementIds.Add(ach.Id);
                result.NewlyDiscoveredAchievements.Add(ach);
            }
        }

        return result;
    }

    public AchievementUnlockResult ProcessTaskCompletion(TodoItem task, UserProfile profile, int totalCompletedTasksCount)
    {
        var result = new AchievementUnlockResult();
        if (task == null || !task.IsCompleted) return result;

        profile.UnlockedAchievements ??= [];
        profile.UnlockedAchievementIds ??= [];
        profile.DiscoveredSecretAchievementIds ??= [];

        var text = NormalizeText(task.Text);
        var nowTime = DateTime.Now.TimeOfDay;
        var todayDay = DateTime.Today.DayOfWeek;

        var allAchievements = GetAllAchievements();

        foreach (var ach in allAchievements)
        {
            if (profile.UnlockedAchievementIds.Contains(ach.Id))
                continue;

            bool isUnlocked = false;

            switch (ach.Id)
            {
                case "tasks_10":
                    if (totalCompletedTasksCount >= 10) isUnlocked = true;
                    break;
                case "tasks_50":
                    if (totalCompletedTasksCount >= 50) isUnlocked = true;
                    break;
                case "tasks_100":
                    if (totalCompletedTasksCount >= 100) isUnlocked = true;
                    break;
                case "tasks_diamond":
                    if (totalCompletedTasksCount >= 200 && profile.CurrentStreak >= 7) isUnlocked = true;
                    break;
                case "streak_7":
                    if (profile.CurrentStreak >= 7) isUnlocked = true;
                    break;
                case "streak_30":
                    if (profile.CurrentStreak >= 30) isUnlocked = true;
                    break;
                case "streak_100":
                    if (profile.CurrentStreak >= 100) isUnlocked = true;
                    break;
                case "geo_explorer":
                    if (task.HasLocationReminder) isUnlocked = true;
                    break;
                case "bio_vault":
                    if (task.IsLocked) isUnlocked = true;
                    break;
            }

            if (!isUnlocked && !string.IsNullOrWhiteSpace(ach.SampleTaskText))
            {
                var sample = NormalizeText(ach.SampleTaskText);
                if (text == sample || (text.Length >= 6 && text.Contains(sample)))
                {
                    isUnlocked = true;
                }
            }

            if (!isUnlocked)
            {
                isUnlocked = CheckSecretRuleMatch(ach.Id, ach.SampleTaskText, text, nowTime, todayDay);
            }

            if (isUnlocked)
            {
                if (!profile.UnlockedAchievements.Contains(ach.Id))
                    profile.UnlockedAchievements.Add(ach.Id);

                if (!profile.UnlockedAchievementIds.Contains(ach.Id))
                    profile.UnlockedAchievementIds.Add(ach.Id);

                profile.Points += 50;
                profile.CurrentXp += 50;
                result.GainedPoints += 50;

                while (profile.CurrentXp >= profile.RequiredXp)
                {
                    profile.CurrentXp -= profile.RequiredXp;
                    profile.Level++;
                    profile.RequiredXp = Math.Round(profile.RequiredXp * 1.25);
                }

                result.NewlyUnlockedAchievements.Add(ach);
            }
        }

        return result;
    }

    private bool CheckSecretRuleMatch(string achId, string? sampleTaskText, string text, TimeSpan nowTime, DayOfWeek todayDay)
    {
        if (!string.IsNullOrWhiteSpace(sampleTaskText))
        {
            var sample = NormalizeText(sampleTaskText);
            if (text == sample || (text.Length >= 6 && text.Contains(sample)))
            {
                return true;
            }
        }

        return achId switch
        {
            "type_reading" => text.Contains("مطالعه") || text.Contains("کتاب"),
            "type_workout" => text.Contains("ورزش") || text.Contains("تمرین") || text.Contains("باشگاه"),
            "type_coding" => text.Contains("برنامه نویسی") || (text.Contains("کد") && text.Length > 4) || text.Contains("دیباگ"),
            "type_selfcare" => text.Contains("سلامت روان") || text.Contains("استراحت کوتاه") || text.Contains("مراقبت"),

            "secret_nightowl" => (text.Contains("نیمه شب") || text.Contains("شبانه")) && (nowTime >= new TimeSpan(2, 0, 0) && nowTime <= new TimeSpan(4, 0, 0)),
            "secret_night_gamer" => (text.Contains("گیم") || text.Contains("بازی")) && (nowTime >= new TimeSpan(1, 0, 0) && nowTime <= new TimeSpan(4, 0, 0)),
            "secret_early_bird" => (text.Contains("بیدارباش") || text.Contains("سحرخیز")) && (nowTime >= new TimeSpan(5, 0, 0) && nowTime <= new TimeSpan(7, 30, 0)),
            "secret_friday_peace" => todayDay == DayOfWeek.Friday && (text.Contains("جمعه") || text.Contains("آرامش")),

            "secret_pizza_lover" => text.Contains("پیتزا"),
            "secret_bug_hunter" => text.Contains("باگ") || text.Contains("دیباگ") || text.Contains("bug"),
            "secret_coffee_addict" => text.Contains("قهوه") || text.Contains("اسپرسو") || text.Contains("coffee"),
            "secret_wanderlust" => text.Contains("مسافرت") || text.Contains("بلیط سفر") || (text.Contains("سفر") && text.Length > 5),
            "secret_music_vibes" => text.Contains("موزیک") || text.Contains("آهنگ") || text.Contains("موسیقی"),
            "secret_mr_fixit" => text.Contains("تعمیر") || text.Contains("درست کردن وسایل"),
            "secret_linux_nerd" => text.Contains("لینوکس") || text.Contains("ترمینال") || text.Contains("linux"),
            "secret_master_chef" => text.Contains("آشپزی") || text.Contains("شام خانگی"),
            "secret_money_saver" => text.Contains("پس انداز") || text.Contains("واریز به حساب"),
            "secret_shopaholic" => text.Contains("خرید وسایل") || text.Contains("خرید هفتگی"),
            "secret_deep_clean" => text.Contains("تمیزکاری") || text.Contains("نظافت میز") || text.Contains("جارو"),
            "secret_social_butterfly" => text.Contains("دیدار با دوستان") || text.Contains("دورهمی رفقا"),
            "secret_polyglot" => text.Contains("زبان انگلیسی") || text.Contains("تمرین لغات"),
            "secret_stay_hydrated" => text.Contains("نوشیدن آب") || text.Contains("لیوان آب"),
            "secret_finance_guru" => text.Contains("فاکتور") || text.Contains("حسابداری") || text.Contains("صورتحساب"),
            "secret_meditation" => text.Contains("مدیتیشن") || text.Contains("ریلکس"),
            "secret_nature_walker" => text.Contains("پیاده روی") || text.Contains("طبیعت گردی"),
            "secret_power_nap" => text.Contains("چرت نیمروزی") || text.Contains("استراحت نیمروزی"),
            "secret_movie_night" => text.Contains("فیلم سینمایی") || text.Contains("سریال"),
            "secret_podcast_time" => text.Contains("پادکست"),
            "secret_ai_master" => text.Contains("هوش مصنوعی") || text.Contains("پرامپت"),
            "secret_gift_self" => text.Contains("پاداش به خود") || text.Contains("هدیه به خود"),
            "secret_digital_detox" => text.Contains("سم زدایی") || text.Contains("دوری از گوشی") || text.Contains("آفلاین"),
            "secret_speed_runner" => text.Contains("فوری و سریع") || text.Contains("کار فوری"),

            "secret_midnight_tea" => (text.Contains("چای") || text.Contains("دمنوش")) && (nowTime >= new TimeSpan(23, 0, 0) || nowTime <= new TimeSpan(4, 0, 0)),
            "secret_clean_desk" => text.Contains("میز کار") || text.Contains("مرتب سازی اتاق"),
            "secret_git_commit" => text.Contains("کامیت") || text.Contains("پوش کد") || text.Contains("git"),
            "secret_family_first" => text.Contains("خانواده") || text.Contains("احوالپرسی"),
            "secret_doc_reader" => text.Contains("داکیومنت") || text.Contains("مستندات"),
            "secret_water_plant" => text.Contains("آب دادن به گل") || text.Contains("گلدان"),
            "secret_backup_hero" => text.Contains("بکاپ") || text.Contains("پشتیبان گیری"),

            _ => false
        };
    }
}