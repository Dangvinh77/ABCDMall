using ABCDMall.Modules.FoodCourt.Application.Helpers;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using ABCDMall.Modules.FoodCourt.Infrastructure.Persistence.FoodCourt;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.FoodCourt.Infrastructure.Seed;

public static class FrontendFoodCourtSeed
{
    public static async Task SeedAsync(FoodCourtDbContext db, CancellationToken ct = default)
    {
        var existing = await db.FoodItems
            .Where(x => FoodSeeds.Select(seed => seed.Id).Contains(x.Id))
            .ToListAsync(ct);

        foreach (var seed in FoodSeeds)
        {
            var item = existing.FirstOrDefault(x => x.Id == seed.Id);
            if (item is null)
            {
                item = new FoodItem { Id = seed.Id };
                await db.FoodItems.AddAsync(item, ct);
                existing.Add(item);
            }

            item.Name = seed.Name;
            item.Slug = FoodHelperSlug(seed.Slug, seed.Name);
            item.Description = seed.Description;
            item.ImageUrl = seed.ImageUrl;
            item.MallSlug = "ABCD Mall";
            item.CategorySlug = seed.CategorySlug;
        }

        await db.SaveChangesAsync(ct);
    }

    private static string FoodHelperSlug(string? slug, string name)
        => string.IsNullOrWhiteSpace(slug) ? SlugHelper.GenerateSlug(name) : SlugHelper.GenerateSlug(slug);

    private sealed record FoodSeed(
        string Id,
        string Name,
        string? Slug,
        string Description,
        string ImageUrl,
        string CategorySlug,
        List<string>? Gallery = null
        
        );

    private static readonly FoodSeed[] FoodSeeds =
    [
            
        new(
            "food-034",
            "Ocean Blue Seafood Buffet",
            "ocean-blue",
            "A premium seafood counter featuring grilled lobster, oysters, sushi, and rotating weekend buffet specials for groups and families.",
            "https://images.unsplash.com/photo-1565680018434-b513d5e5fd47?q=80&w=1600&auto=format&fit=crop",
            "seafood"),
        new(
            "food-035",
            "Boba Bella Milk Tea",
            "boba-bella",
            "Fresh milk tea, fruit tea, and signature brown sugar boba served all day from the central beverage kiosk.",
            "/img/Boba Bella Milk Tea/logo.jpg",
            "drinks"),
        new(
            "food-036",
            "Babushka A La Carte",
            "babushka-a-la-carte",
            "An international comfort-food station with pasta, baked rice, and lunch set menus designed for office crowds.",
            "https://images.unsplash.com/photo-1504674900247-0877df9cc836?q=80&w=1600&auto=format&fit=crop",
            "international"),
        new(
            "food-037",
            "Saigon Grill Express",
            "saigon-grill-express",
            "Vietnamese grilled specialties with fast-service rice plates, noodles, and flame-cooked meat skewers.",
            "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?q=80&w=1600&auto=format&fit=crop",
            "vietnamese"),
        new(
            "food-038",
            "Tokyo Ramen Station",
            "tokyo-ramen-station",
            "Japanese ramen bowls, gyoza, and crispy karaage prepared in an open-kitchen concept near the family seating area.",
            "https://images.unsplash.com/photo-1617093727343-374698b1b08d?q=80&w=1600&auto=format&fit=crop",
            "japanese"),

            new(
            "food-001","STARBUCKS",
            "starbucks",
            "Starbucks Coffee Company  Every week, Starbucks serves millions of beverages to customers around the globe. Yet, Starbucks remains committed to serving each individual customer, one person at a time.  Every Starbucks beverage is hand-delivered to customers right at the store's bar. This represents the vital connection between Starbucks and our customers.  Starbucks pledges that this connection stems from our deep appreciation—we are committed to serving the highest quality coffee products, fostering a personal bond with every customer, and fulfilling our social and community responsibilities wherever Starbucks operates.  Beginning as a single coffee shop more than 50 years ago, Starbucks now strives to improve and create a better environment in every country and every location we enter, surpassing even our own previous achievements.",
            "/img/starbuck/logo.png",
            "coffee"
            ),

            new(
            "food-002",
            "HIGHLANDS COFFEE",
            "highlands-coffee",
            "About Highlands Coffee - A Brand Rooted in Vietnamese Coffee  Born from a love for Vietnam and a passion for coffee, the Highlands Coffee® brand was established in 1999 with the aspiration to elevate Vietnam's long-standing coffee heritage and spread a spirit of pride, harmoniously connecting tradition with modernity.  To enjoy Highlands Coffee's delightful beverages, you simply need to pick up the phone and call the hotline at 1800 6938, or order online by accessing their platform, choosing your favorite items, and then just waiting for \"joy\" to knock on your door.",
            "/img/2lands/logo.png",
            "coffee"
            ),

            new(
            "food-003",
            "Gogi House",
            "gogi-house",
            "Gogi House is a restaurant chain featuring the signature Korean tabletop BBQ style. Gogi House is constantly striving to provide diners with the ultimate BBQ experience, boasting authentic Korean flavors. To enhance the taste of the meat, Gogi House has developed a unique and exclusive marinade recipe. Furthermore, the meat is marinated for 48 hours, ensuring the flavors successfully penetrate every fiber. At Gogi House, you can enjoy delicious grilled dishes served by enthusiastic and hardworking staff in a cozy, rustic atmosphere. To learn more details about Gogi House, join Vincom in exploring the review section below! Pro tip: Don’t forget to book your table in advance at Gogi House to receive the most attentive guidance and service.",
            "/img/gogi/logo.png",
            "korean"
            ),

            new(
            "food-004",
            "Taipan",
            "taipan",
            "Taipan is a restaurant specializing in Hong Kong cuisine, particularly known for its authentic Dim Sum and signature roasted dishes like Peking duck. It offers diners a premium culinary experience reminiscent of traditional Hong Kong flavors.",
            "/img/taipan/logo.jpg",
            "chinese"
            ),

            new(
            "food-005",
            "Sushi Kei",
            "sushi-kei",
            "Sushi Kei is a Japanese restaurant chain that delivers authentic and high-quality Japanese cuisine. The word \"Kei\" means joy, reflecting the brand's mission to bring happiness to diners through meticulously crafted sushi, sashimi, and traditional dishes.",
            "/img/sushikei/logo.webp",
            "japanese"
            ),

            new(
            "food-006",
            "SushiX",
            "sushix",
            "SushiX is a modern conveyor belt sushi brand offering a novel uniform-pricing model. It provides a fast, fresh, and highly affordable Japanese culinary experience, making famous delicacies accessible to everyone.",
            "/img/sushix/logo.jpg",
            "japanese"
            ),

            new(
            "food-007",
            "WOW! Yakiniku",
            "wow-yakiniku",
            "Inspired by the vibrant and fast-paced lifestyle of Japan, WOW! Yakiniku is a \"Solo Grill\" restaurant. It offers a personalized dining journey where individuals can comfortably enjoy premium Japanese BBQ in their own private space.",
            "/img/wowyakiniku/logo.png",
            "japanese"
            ),

            new(
            "food-008",
            "Dao Niu Guo",
            "dao-niu-guo",
            "Dao Niu Guo is a casual dining restaurant chain specializing in Chaoshan-style fresh beef hot pot. The brand prides itself on using fresh beef imported daily and hand-sliced at the counter, paired with a clear, sweet broth for an authentic taste.",
            "/img/daoniuguo/logo.jpg",
            "chinese"
            ),

            new(
            "food-009",
            "Texas Chicken",
            "texas-chicken",
           "Texas Chicken is a famous American fast-food chain globally recognized for its signature crispy, juicy fried chicken and honey-butter biscuits. The brand is committed to serving high-quality, freshly prepared meals with bold Texan flavors.",
            "/img/texaschicken/logo.jpg",
            "fastfood"
            ),

            new(
            "food-010",
            "Hoang Yen Buffet",
            "hoang-yen-buffet",
            "Hoàng Yến Buffet is a premier Vietnamese dining destination offering a vast selection of traditional and contemporary local dishes. It provides a high-class buffet experience that celebrates the rich and diverse culinary heritage of Vietnam.",
            "/img/buffethoangyen/logo.webp",
            "vietnamese"
            ),

            new(
            "food-011",
            "ThaiExpress",
            "thaiexpress",
            "ThaiExpress is a modern restaurant chain that brings the authentic taste of Thailand to diners in a comfortable setting. It is famous for its vibrant dishes that perfectly balance the signature sweet, sour, spicy, and salty notes of Thai cuisine.",
            "/img/thaiexpress/logo.webp",
            "thai"
            ),

            new(
            "food-012",
            "Khao Lao",
            "khao-lao",
            "Khao Lao is a pioneering restaurant chain dedicated to modern Laotian cuisine. It elegantly blends traditional Laotian recipes with contemporary culinary techniques, offering diners a harmonious, rustic, yet uniquely flavorful dining experience.",
            "/img/khaolao/logo.webp",
            "lao"
            ),

            new(
            "food-013",
            "Chang Modern Thai Cuisine",
            "chang-modern-thai-cuisine",
            "Chang is a dynamic restaurant brand offering a modern twist on classic Thai food. It combines fresh ingredients with authentic spices to deliver vibrant, bold flavors in a stylish and youthful environment.",
            "/img/chang/logo.webp",
            "thai"
            ),


            new(
            "food-014",
            "Bonchon",
            "bonchon",
            "Bonchon is a globally acclaimed South Korean fried chicken franchise. It is best known for its signature double-frying technique and proprietary savory glazes, delivering an irresistibly crispy exterior and juicy interior.",
            "/img/bonchon/logo.jpg",
            "korean"
            ),

            new(
            "food-015",
            "Crystal Jade",
            "crystal-jade",
            "Crystal Jade is an internationally recognized, Michelin-awarded restaurant brand specializing in authentic Cantonese cuisine. It is celebrated for its premium Dim Sum, roasted meats, and traditional wok-fried dishes served in a sophisticated setting.",
            "/img/crystaljade/logo.png",
            "chinese"
            ),

            new(
            "food-016",
            "Kichi-Kichi",
            "kichi-kichi",
            "Kichi-Kichi is a leading conveyor belt hot pot chain in Vietnam. Operating on an \"all-you-can-eat\" model, it offers a fun and dynamic dining experience with a continuous flow of fresh meats, seafood, and vegetables moving along a vibrant Kaiten conveyor belt.",
            "/img/kichi/logo1.png",
            "hotpot"
            ),

            new(
            "food-017",
            "The Pizza Company",
            "the-pizza-company",
            "The Pizza Company is one of the fastest-growing international pizza brands in Vietnam, offering a unique fusion of Italian culinary art and sophisticated Asian flavors. The brand is particularly famous for its Premium Seafood Pizza line, featuring high-quality cheese and hand-pushed dough. With its warm and inviting ambiance, it serves as an ideal destination for family gatherings and social meetups.",
            "/img/pizzaCompany/logo.webp",
            "pizza"
            ),

            new(
            "food-018",
            "Dookki Vietnam",
            "dookki-vietnam",
            "Dookki is the pioneer that brought the authentic Korean Topokki buffet model to Vietnam, creating a massive trend among the youth. Here, diners are more than just guests; they become \"chefs\" who can personally craft their own rice cake hotpot using exclusive sauce recipes and over 30 diverse ingredients, ranging from cheese-filled rice cakes and fish cakes to fresh seafood.",
             "/img/Dookki/logo.png",
             "korean"
            //  new List<string>
            // {
            //     "/img/Dookki/logo1.jpg",
            //     "/img/Dookki/logo2.jpeg"
            // }

            
            ),

            new(
            "food-019",
            "King BBQ",
            "king-bbq",
            "Holding the title \"King of Korean BBQ,\" King BBQ has solidified its leading position in Vietnam's grill market. The defining factor of the brand is \"King Sauce\" – a secret recipe developed by top Korean chefs that enhances the natural flavors of premium American beef and pork while maintaining a bold, authentic traditional taste.",
            "/img/King BBQ/logo.jpg",
            "bbq"
            ),

            new(
            "food-020",
            "Lok Lok Hotpot",
            "lok-lok-hotpot",
            "Lok Lok Hotpot offers a modern street-food experience through its colorful and vibrant skewer-based hotpot model. Diners are free to choose from dozens of varieties of skewers—including meatballs, seafood, and vegetables—which are then dipped into rich, flavorful broths featuring distinct Asian profiles. It is an excellent choice for those seeking innovation and variety in a single meal.",
            "/img/Lok Lok Hotpot/logo.png",
            "hotpot"
            ),

            new(
            "food-021",
            "Hot Pot Story",
            "hot-pot-story",
            "Hot Pot Story prides itself on being a hub for international hotpot excellence, allowing diners to journey through the culinary cultures of Thailand, Japan, Korea, and China right at their table. With five legendary broth flavors ranging from spicy Tomyum to light herbal soups, paired with imported American beef and fresh vegetables, the restaurant is committed to delivering a complete and high-quality buffet experience.",
            "/img/Hot Pot Story/logo.jpg",
            "hotpot"
            ),

            new(
            "food-022",
            "Chilli Thai",
            "chilli-thai",
            "Chilli Thai stands as an icon of modern Thai cuisine, where every dish is a work of art perfectly balancing the four essential flavors: sour, spicy, salty, and sweet. From street-food classics like Pad Thai to sophisticated banquet dishes like Green Curry, Chilli Thai uses spices imported directly from Thailand to ensure the preservation of the vibrant and intense flavors of the Golden Pagoda land.",
            "/img/Chilli Thai/logo.png",
            "thai"
            ),

            new(
            "food-023",
            "Shabu Ya",
            "shabu-ya",
            "Shabu Ya is a Japanese hotpot buffet restaurant specializing in the traditional Shabu Shabu style. It is famous for serving premium beef slices dipped in delicate, nutritious, and health-conscious Japanese broths.",
            "/img/Shabu Ya/logo.png",
            "japanese"
            ),

            new(
            "food-024",
            "H BBQ Buffet",
            "h-bbq-buffet",
            "H BBQ Buffet is a highly favored BBQ buffet spot (especially in the Bien Hoa, Dong Nai area). The restaurant scores highly for its fresh ingredients, quickly refilled food stations, prompt service, and polite dining atmosphere.",
            "/img/H BBQ Buffet/logo.jpg",
            "bbq"
            ),

            new(
            "food-025",
            "Marukame Udon",
            "marukame-udon",
            "Marukame Udon is the world’s leading Udon and Tempura restaurant chain, rooted in the culinary heritage of the Sanuki region in Japan. The brand is famous for its unique open-kitchen concept and self-service dining style. The hallmark of Marukame Udon is its commitment to freshness; the noodles are kneaded and freshly made in-store every day to ensure the perfect chewy texture. Combined with delicate, savory Dashi broth and a variety of crispy golden tempuras, Marukame Udon provides an authentic yet modern Japanese dining experience that is both fast and sophisticated.",
            "/img/Marukame Udon/logo.webp",
            "japanese"
            ),

            new(
            "food-026",
            "Mikado Sushi",
            "mikado-sushi",
            "Mikado Sushi is one of the leading brands in the conveyor belt sushi (Kaiten Sushi) industry, best known for its highly attractive uniform-pricing model. With the philosophy of making premium Japanese cuisine accessible to a wider audience, Mikado Sushi places a strong emphasis on the freshness of ingredients, which are carefully selected every day. The menu is incredibly diverse, featuring hundreds of types of sushi, sashimi, and side dishes prepared by an experienced culinary team. The modern atmosphere combined with the continuous motion of sushi plates on the conveyor belt creates a dynamic, exciting, and colorful dining experience.",
            "/img/Mikado Sushi/logo.png",
            "japanese"
            ),

            new(
            "food-027",
            "Yamazaki Bakery",
            "yamazaki-bakery",
            "Yamazaki Bakery is the pride of Japan’s baking industry, established in 1948. With over 70 years of history, it has grown to become the largest bread and pastry corporation in the Land of the Rising Sun. The brand is renowned for perfectly blending traditional Western baking techniques with the meticulousness and subtle flavors of Japanese craftsmanship. In Vietnam, Yamazaki Bakery attracts customers with fresh pastries baked on-site daily. It is particularly famous for its cloud-soft \"shokupan\" (white bread) and a diverse selection of sweet and savory buns that embody a distinct Japanese flair.",
            "/img/Yamazaki Bakery/logo.jpg",
            "bakery"
            ),

            new(
            "food-028",
            "BobaPop",
            "bobapop",
            "BobaPop is a fresh tea brand originating from Taiwan that officially entered the Vietnamese market during the initial bubble tea boom. Featuring its iconic and playful Bulldog mascot, BobaPop quickly won the hearts of the youth with its modern and dynamic style. The brand emphasizes the use of high-quality, fresh tea leaves and an on-site brewing process to preserve the authentic, natural flavors. BobaPop offers an extensive menu, ranging from creamy foam teas and refreshing fruit teas to a diverse array of toppings, providing a perfect harmony between traditional Taiwanese flavors and contemporary trends.",
            "/img/BobaPop/logo.jpg",
            "drinks"
            ),

            new(
            "food-029",
            "Tasaki BBQ",
            "tasaki-bbq",
            "Tasaki BBQ is a premium restaurant chain specializing in traditional Japanese charcoal-grilled Yakiniku. The most distinctive feature that sets Tasaki BBQ apart is the use of smokeless charcoal inside modern grilling stations, which perfectly preserves the natural flavor of the food without leaving any lingering odors. The restaurant is renowned for its high-quality ingredients, particularly its Wagyu beef featuring exquisite marbling and signature sauces crafted from fresh fruits. The ambiance at Tasaki BBQ deeply reflects Japanese culture—sophisticated yet warm—making it an ideal venue for business meetings or intimate family gatherings.",
            "/img/Tasaki BBQ/logo.png",
            "bbq"
            ),

            new(
            "food-030",
            "Joopii",
            "joopii",
             "Joopii is a trendy buffet restaurant chain specializing in Topokki hotpot and Korean fried chicken, capturing the vibrant essence of modern Korean street food. Although a newer player in the culinary scene, Joopii has quickly made a mark with its \"all-you-can-eat\" model set in a youthful and energetic environment. At Joopii, diners can get creative by mixing their own hotpot sauces from a diverse sauce bar, paired with cheese-filled rice cakes, seafood, and fresh vegetables. A major highlight of the brand is its crispy fried snacks and flavorful glazed chicken wings, offering a comprehensive Korean feast at a very affordable price point, making it a top choice for students and young foodies.",
            "/img/Joopii/logo.png",
            "korean"
            ),

            new(
            "food-031",
            "Chang Kang Kung",
            "chang-kang-kung",
            "Chang Kang Kung is the pioneer of the \"Hydro-thermal Steaming\" (Hong Kong Steam) trend in Vietnam, introducing a fresh perspective on health-conscious dining. With the philosophy \"Eat for Health,\" the restaurant utilizes advanced high-temperature steam technology to cook food right at the table. This method ensures that all vitamins, minerals, and the natural sweetness of the ingredients are perfectly preserved without the need for cooking oil. The menu at Chang Kang Kung is diverse, featuring everything from fresh seafood and premium beef to delicate dim sum and vibrant vegetables, all coming together to create a light, flavorful, and nutrient-rich dining experience.",
            "/img/Chang Kang Kung/logo.jpg",
            "chinese"
            ),

            new(
            "food-032",
            "Mei Wei",
            "mei-wei",
            "Mei Wei, which translates to \"Delicious Flavor\" in Chinese, is a restaurant chain specializing in authentic Dim Sum and steamed seafood, deeply rooted in traditional Chinese culinary arts. With the ambition to bring the colorful world of Chinese cuisine closer to diners, Mei Wei focuses on the perfect harmony of color, aroma, and taste. The menu is exceptionally diverse, featuring over 50 meticulously crafted Dim Sum dishes, alongside roasted duck, steamed chicken, and fresh seafood. A key highlight of Mei Wei is its modern Chinese-inspired architectural design, combined with an open-kitchen concept that allows guests to witness steaming baskets of hot Dim Sum delivered directly to their table.",
            "/img/Mei Wei/logo.webp",
            "chinese"
            ),

            new(
            "food-033",
            "Lẩu Nướng 88",
            "lu-nuong-88",
           "Lẩu Nướng 88 is a popular buffet brand known as a culinary paradise for those who love the combination of hotpot and BBQ. With a spacious, youthful, and modern environment, the restaurant offers an extensive menu of hundreds of dishes, reflecting a sophisticated blend of Japanese, Korean, Chinese, and Thai culinary cultures. The main attraction of Lẩu Nướng 88 is the vast variety of fresh beef, seafood, and a vibrant ready-to-eat station featuring everything from sushi and dim sum to eye-catching desserts. With affordable pricing and dedicated service, it has become a top choice for birthday parties, celebrations, and casual social gatherings.",
            "/img/Lẩu Nướng 88/logo.jpg",
            "bbq"
            )
    ];

       
}

