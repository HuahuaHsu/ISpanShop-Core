using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ISpanShop.Models.EfModels; // 確保對應到你的 EF Models 命名空間

namespace ISpanShop.Models
{
	/// <summary>
	/// 電商資料播種程式 - 從公開 API (DummyJSON) 串接真實商品資料
	/// </summary>
	public class DataSeeder
	{
		private static readonly Random _random = new Random();
		private const string DUMMYJSON_URL = "https://dummyjson.com/products?limit=194"; // 已經幫你擴充到 194 筆
		private const decimal USD_TO_TWD = 30;  // 匯率：美金轉台幣

		/// <summary>
		/// 父子多層式分類對應表 (階層分類)
		/// Dictionary<API原始分類, (主分類名稱, 子分類名稱)>
		/// </summary>
		private static readonly Dictionary<string, (string ParentName, string ChildName)> CategoryHierarchyMap = new()
		{
            // === 美妝與保養 ===
            { "beauty", ("美妝與保養", "彩妝與修容") },
			{ "fragrances", ("美妝與保養", "香水與香氛") },
			{ "skin-care", ("美妝與保養", "臉部與身體保養") },
            
            // === 居家生活 ===
            { "furniture", ("居家與生活", "大型家具") },
			{ "home-decoration", ("居家與生活", "居家裝飾與收納") },
			{ "kitchen-accessories", ("居家與生活", "廚房餐具與用品") },
			{ "groceries", ("美食與生鮮", "生鮮食材與飲品") },
            
            // === 3C與電子 ===
            { "laptops", ("3C與電子", "筆記型電腦") },
			{ "smartphones", ("3C與電子", "智慧型手機") },
			{ "tablets", ("3C與電子", "平板電腦") },
			{ "mobile-accessories", ("3C與電子", "手機與平板周邊") },
            
            // === 時尚男裝 ===
            { "mens-shirts", ("男裝與配件", "男款上衣與襯衫") },
			{ "mens-shoes", ("男裝與配件", "男士鞋款") },
			{ "mens-watches", ("男裝與配件", "男士腕錶") },
            
            // === 時尚女裝 ===
            { "tops", ("女裝與配件", "女款上衣與洋裝") },
			{ "womens-dresses", ("女裝與配件", "派對與晚禮服") },
			{ "womens-bags", ("女裝與配件", "精品包包") },
			{ "womens-shoes", ("女裝與配件", "女款鞋類") },
			{ "womens-watches", ("女裝與配件", "女士腕錶") },
			{ "womens-jewellery", ("女裝與配件", "珠寶與飾品") },
            
            // === 戶外與配件 ===
            { "sunglasses", ("運動與休閒", "太陽眼鏡與配件") },
			{ "sports-accessories", ("運動與休閒", "運動裝備與球類") },
            
            // === 汽機車 ===
            { "motorcycle", ("汽機車百貨", "機車與周邊配件") },
			{ "vehicle", ("汽機車百貨", "汽車與周邊配件") }
		};

		/// <summary>
		/// 品牌名稱翻譯表
		/// 幫你把常見大廠翻譯成台灣習慣的說法
		/// </summary>
		private static readonly Dictionary<string, string> BrandTranslationMap = new(StringComparer.OrdinalIgnoreCase)
		{
            // 3C 與科技
            { "Apple", "蘋果 (Apple)" },
			{ "Samsung", "三星 (Samsung)" },
			{ "Huawei", "華為 (Huawei)" },
			{ "Lenovo", "聯想 (Lenovo)" },
			{ "Dell", "戴爾 (Dell)" },
			{ "Asus", "華碩 (ASUS)" },
			{ "Oppo", "OPPO" },
			{ "Realme", "realme" },
			{ "Vivo", "vivo" },
			{ "Amazon", "亞馬遜 (Amazon)" },
			{ "Beats", "Beats by Dr. Dre" },
			{ "Gigabyte", "技嘉 (AORUS)" },
            
            // 精品與香水
            { "Chanel", "香奈兒 (Chanel)" },
			{ "Dior", "迪奧 (Dior)" },
			{ "Dolce & Gabbana", "Dolce & Gabbana (D&G)" },
			{ "Gucci", "古馳 (Gucci)" },
			{ "Prada", "普拉達 (Prada)" },
			{ "Calvin Klein", "Calvin Klein (CK)" },
			{ "Rolex", "勞力士 (Rolex)" },
			{ "Longines", "浪琴 (Longines)" },
			{ "IWC", "萬國錶 (IWC)" },
            
            // 運動與服飾
            { "Nike", "耐吉 (Nike)" },
			{ "Puma", "彪馬 (PUMA)" },
			{ "Off White", "Off-White" },
            
            // 汽機車
            { "Kawasaki", "川崎 (Kawasaki)" },
			{ "Chrysler", "克萊斯勒 (Chrysler)" },
			{ "Dodge", "道奇 (Dodge)" },
            
            // 日用品與美妝
            { "Olay", "歐蕾 (Olay)" },
			{ "Vaseline", "凡士林 (Vaseline)" },
			{ "Attitude", "Attitude 天然護理" },
			{ "Essence", "Essence 艾森絲" }
		};

		
		private static readonly Dictionary<string, (string Title, string Description)> ProductTranslationMap = new()
		{
			// === 💄 美妝與護膚 (Cosmetics & Skincare) ===
    {"Essence Mascara Lash Princess", ("精華纖長睫毛膏", "超人氣濃密纖長睫毛膏，持久防水配方讓您輕鬆打造無死角的迷人電眼。")},
	{"Eyeshadow Palette with Mirror", ("自帶補妝鏡多色眼影盤", "百搭實用色系一次擁有！粉質細膩服貼，內附實用化妝鏡，隨時隨地保持完美妝容。")},
	{"Powder Canister", ("無瑕控油定妝蜜粉", "輕盈透氣的細緻粉末，長效控油不暗沉，為您打造宛如天生的微霧面平滑底妝。")},
	{"Red Lipstick", ("經典正紅誘惑唇膏", "絲絨奶霜質地，一抹極致顯色，為雙唇注入飽滿色彩，瞬間提升全場氣場。")},
	{"Red Nail Polish", ("勃艮第紅指甲油", "沙龍級高光澤亮面，快乾持久不掉色，讓指尖散發迷人的成熟女人味。")},
    
    // === 🌸 香水 (Fragrances) ===
    {"Calvin Klein CK One", ("CK One 中性淡香水", "經典不敗的清新綠茶柑橘調，男女皆宜，散發隨性又充滿活力的獨特魅力。")},
	{"Chanel Coco Noir Eau De", ("香奈兒黑色可可淡香水", "揉合葡萄柚、玫瑰與溫潤檀香，神祕且優雅，是夜晚約會或晚宴的完美選擇。")},
	{"Dior J'adore", ("Dior 真我宣言淡香水", "奢華揉合依蘭依蘭、玫瑰與頂級茉莉，完美詮釋女性的極致優雅與自信。")},
	{"Dolce Shine Eau de", ("D&G 閃耀女性淡香水", "充滿陽光氣息的芒果與茉莉花果香調，帶來猶如漫步在義大利海岸的愉悅心情。")},
	{"Gucci Bloom Eau de", ("Gucci 花悅女性淡香水", "以晚香玉與茉莉交織出宛如盛開花園的香氣，展現現代女性的浪漫與柔美。")},
    
    // === 🛋️ 家具與居家裝飾 (Furniture & Home Decor) ===
    {"Annibale Colombo Bed", ("義大利經典雙人床架", "頂級材質結合匠心工藝，為您的臥室注入奢華質感，提供最極致的睡眠體驗。")},
	{"Annibale Colombo Sofa", ("義大利頂級真皮沙發", "精緻設計與頂級皮革的完美相遇，坐感舒適，是客廳品味升級的絕佳焦點。")},
	{"Bedside Table African Cherry", ("非洲櫻桃木床頭櫃", "保留珍貴木材天然紋理，兼具實用收納與優雅設計，提升整體空間溫馨感。")},
	{"Knoll Saarinen Executive Conference Chair", ("高階主管人體工學椅", "世紀中期現代主義經典之作！完美支撐背部曲線，辦公與會議室的質感首選。")},
	{"Wooden Bathroom Sink With Mirror", ("實木衛浴浴櫃附化妝鏡", "防潮實木打造，獨特質感設計搭配專屬浴鏡，讓衛浴空間煥然一新。")},
	{"Decoration Swing", ("北歐風手工編織吊籃", "精緻編織細節，為居家空間增添一抹童話般的優雅與波西米亞度假風情。")},
	{"Family Tree Photo Frame", ("家族樹造型相框", "以質感樹木造型展示多張珍貴照片，讓溫馨的家庭回憶成為最美的居家裝飾。")},
	{"House Showpiece Plant", ("擬真居家裝飾盆栽", "無需澆水照料即可擁有滿滿綠意，為室內空間注入自然生機與現代設計感。")},
	{"Plant Pot", ("極簡風現代質感花盆", "俐落流暢的線條設計，完美襯托各式植栽，無論放室內外都能提升空間品味。")},
	{"Table Lamp", ("護眼金屬質感檯燈", "兼具照明與裝飾功能，柔和光源營造溫馨氛圍，為您的閱讀或工作時光加分。")},

    // === 🥩 生鮮與食品 (Groceries) ===
    {"Apple", ("產地直送鮮脆蘋果", "每日清晨新鮮採摘，果肉鮮甜多汁，富含豐富維他命C，健康活力的最佳來源。")},
	{"Beef Steak", ("極黑和牛霜降牛排", "完美大理石油花分布，肉質軟嫩多汁，讓您在家也能輕鬆煎出五星級美味。")},
	{"Cat Food", ("無穀頂級鮮肉貓糧", "專為挑嘴貓主子設計，高含肉量且無添加穀物，全面滿足愛貓的營養需求。")},
	{"Chicken Meat", ("去骨本土仿土雞腿肉", "嚴選放山雞，肉質Q彈有嚼勁，適合煮湯、煎烤，是家庭料理的百搭食材。")},
	{"Cooking Oil", ("冷壓初榨頂級橄欖油", "100% 第一道冷壓榨取，保留最豐富的營養精華，適合涼拌與低溫健康烹調。")},
	{"Cucumber", ("溫室無毒脆甜小黃瓜", "網室精心栽培無農藥殘留，口感清脆爽口，生菜沙拉或涼拌菜的最佳主角。")},
	{"Dog Food", ("全齡犬深海魚狗糧", "富含必需營養素與 Omega-3，保護關節與毛髮，讓愛犬每天充滿健康活力。")},
	{"Eggs", ("友善平飼放牧紅殼蛋", "來自快樂母雞的健康好蛋，蛋黃飽滿濃郁無抗生素，適合各式烘焙與料理。")},
	{"Fish Steak", ("挪威空運厚切鮭魚片", "來自純淨海域的頂級漁獲，油脂豐富入口即化，簡單乾煎即可品嚐極致海味。")},
	{"Green Bell Pepper", ("產銷履歷翠綠青椒", "色澤鮮綠且果肉厚實，富含膳食纖維，為您的熱炒料理增添豐富色彩與口感。")},
	{"Green Chili Pepper", ("特級辛香青辣椒", "辣度適中並帶有獨特清香，適合爆香或製作特製醃料，瞬間提升料理層次。")},
	{"Honey Jar", ("台灣龍眼純天然蜂蜜", "100% 純天然無添加，蜜香濃郁色澤琥珀，無論沖泡飲品或烘焙甜點皆宜。")},
	{"Ice Cream", ("法式香草莢冰淇淋", "採用頂級天然香草莢製作，口感綿密順滑，帶給您夏日最奢華的甜品享受。")},
	{"Juice", ("100% 鮮榨綜合果汁", "滿滿維他命與天然果香，無添加人工糖分，是您早晨解渴與補充活力的首選。")},
	{"Kiwi", ("紐西蘭特級奇異果", "營養密度極高，酸甜多汁，無論直接享用或加入優格沙拉，都是健康好滋味。")},
	{"Lemon", ("屏東無籽綠檸檬", "酸度十足且香氣濃郁，適合入菜、烘焙或製作清涼解渴的蜂蜜檸檬飲品。")},
	{"Milk", ("在地小農鮮乳", "每日清晨產地直送，保留最純粹的濃郁奶香與豐富鈣質，全家人健康的好夥伴。")},
	{"Mulberry", ("有機手採甜黑桑葚", "果實飽滿多汁，富含花青素與抗氧化物，當作零食或熬煮果醬都非常適合。")},
	{"Nescafe Coffee", ("雀巢金牌醇厚咖啡", "嚴選咖啡豆烘焙而成，香氣濃郁口感滑順，為您帶來早晨最美好的醒腦時光。")},
	{"Potatoes", ("產銷履歷優質馬鈴薯", "口感綿密鬆軟，無論是烤馬鈴薯、煮濃湯或做成馬鈴薯泥，都是百搭神級食材。")},
	{"Protein Powder", ("高蛋白乳清沖泡飲", "提供優質且好吸收的蛋白質，是健身族群肌肉修復與日常營養補充的最佳選擇。")},
	{"Red Onions", ("紫皮鮮甜紅洋蔥", "香氣濃郁且辛辣度低，富含豐富微量元素，是提升燉肉與沙拉風味的秘密武器。")},
	{"Rice", ("花東特級香米", "米粒飽滿透亮，煮熟後散發淡淡芋頭香氣，口感Q彈，是各式台菜的完美基底。")},
	{"Soft Drinks", ("經典氣泡汽水綜合組", "多種暢快口味一次滿足，氣泡強烈清涼解渴，是派對聚餐不可或缺的歡樂飲品。")},
	{"Strawberry", ("大湖直送鮮採草莓", "果實碩大紅潤，酸甜多汁香氣撲鼻，無論單吃或製作甜點都能帶來滿滿幸福感。")},
	{"Tissue Paper Box", ("純水柔韌抽取式衛生紙", "親膚觸感且不易破，不含螢光劑，為您與家人提供最安心柔軟的日常清潔。")},
	{"Water", ("深海純淨微礦泉水", "經過多重過濾與殺菌，蘊含微量礦物質，口感甘甜順口，隨時補充流失水分。")},

    // === 🍳 廚房用品 (Kitchenware) ===
    {"Bamboo Spatula", ("天然竹製不沾鍋鍋鏟", "環保天然竹材製成，耐高溫且不傷鍋面，手感溫潤，是翻炒料理的得力助手。")},
	{"Black Aluminium Cup", ("霧黑鋁合金冷水杯", "極簡輕量化設計，導冷極快，讓您在炎炎夏日享受最透心涼的飲品體驗。")},
	{"Black Whisk", ("人體工學矽膠打蛋器", "防滑握把搭配耐熱矽膠，輕鬆打發蛋液與奶油，不刮傷打蛋盆，清洗超方便。")},
	{"Boxed Blender", ("多功能隨行果汁機", "強勁馬力輕鬆擊碎冰塊，附贈隨行杯蓋，讓您隨時隨地享受新鮮現打的健康蔬果汁。")},
	{"Carbon Steel Wok", ("無塗層碳鋼中華炒鍋", "傳熱迅速均勻，經開鍋保養後可形成天然不沾層，完美鎖住食材的爆炒鍋氣。")},
	{"Chopping Board", ("抗菌防滑實木砧板", "採用高密度原木壓製，不易留刀痕與發霉，提供最安全衛生的食材處理環境。")},
	{"Citrus Squeezer Yellow", ("手動檸檬鮮果壓汁器", "亮眼色彩搭配省力槓桿設計，輕鬆榨取每一滴新鮮果汁，完整保留維生素C。")},
	{"Egg Slicer", ("不鏽鋼切蛋器", "輕輕一壓即可切出完美均勻的蛋片，是製作三明治、沙拉與冷盤的必備神器。")},
	{"Electric Stove", ("便攜式多功能電磁爐", "輕巧不佔空間，支援多段火力調節與定時功能，租屋族與小家庭的料理好幫手。")},
	{"Fine Mesh Strainer", ("不鏽鋼細目漏勺過濾網", "網格極細緻，過濾高湯、撈浮沫或篩麵粉都超級好用，確保料理口感細滑。")},
	{"Fork", ("經典不鏽鋼餐叉", "優質加厚不鏽鋼打造，線條流暢握感厚實，為您的餐桌增添一絲優雅氣息。")},
	{"Glass", ("北歐風透明玻璃水杯", "清透無鉛玻璃材質，極簡造型，完美展現各類飲品的漸層色澤與清涼感。")},
	{"Grater Black", ("多功能四面刨絲器", "鋒利耐用，無論是起司、蘿蔔絲或檸檬皮都能輕鬆刨削，大幅縮短備料時間。")},
	{"Hand Blender", ("手持式攪拌棒四件組", "攪拌、切碎、打發一機搞定，體積輕巧好收納，製作副食品或濃湯的最佳幫手。")},
	{"Ice Cube Tray", ("食品級矽膠製冰盒", "柔軟材質讓脫模超級輕鬆，附帶防塵蓋設計，確保冰塊無異味，炎夏必備。")},
	{"Kitchen Sieve", ("烘焙專用細緻麵粉篩", "均勻過篩結塊麵粉，讓蛋糕與餅乾口感更加蓬鬆細緻，烘焙新手的完美工具。")},
	{"Knife", ("主廚級大馬士革切片刀", "鋒利無比且配重完美，輕鬆處理各類肉品與蔬菜，體驗行雲流水般的刀工。")},
	{"Lunch Box", ("日系分隔微波便當盒", "防漏設計且分格不串味，材質安全可直接微波加熱，讓帶便當成為一種享受。")},
	{"Microwave Oven", ("智能微電腦微波爐", "多段微波火力與內建多種自動烹調模式，加熱、解凍迅速均勻，廚房效率神機。")},
	{"Mug Tree Stand", ("實木馬克杯收納掛架", "穩固不傾倒，有效利用桌面垂直空間，同時展示您心愛的馬克杯收藏。")},
	{"Pan", ("麥飯石不沾平底鍋", "採用醫療級不沾塗層，只需少油即可煎出完美荷包蛋，清洗只需一抹即淨。")},
	{"Plate", ("陶瓷簡約西餐盤", "高溫燒製釉面光滑，耐磨防刮，完美的留白設計讓每道家常菜都像米其林大餐。")},
	{"Red Tongs", ("耐高溫矽膠料理夾", "防燙防滑握把，夾取熱食或翻麵煎肉超順手，前端矽膠設計保護鍋具不刮傷。")},
	{"Silver Pot With Glass Cap", ("304不鏽鋼雙耳湯鍋附蓋", "導熱快且保溫效果好，透明玻璃鍋蓋讓烹煮進度一目了然，燉湯滷肉必備。")},
	{"Slotted Turner", ("不鏽鋼鏤空煎魚鏟", "濾油鏤空設計，極薄鏟口輕鬆翻面不破皮，讓您煎魚、煎牛排更加得心應手。")},
	{"Spice Rack", ("旋轉式調味料收納架", "包含多個透明玻璃調味罐，360度旋轉拿取超方便，讓廚房檯面告別雜亂。")},
	{"Spoon", ("圓潤不鏽鋼喝湯匙", "邊緣打磨光滑不刮嘴，深度適中，無論是喝濃湯或吃甜品都能擁有極佳手感。")},
	{"Tray", ("防滑皮革把手托盤", "木紋質感搭配防滑表面，無論是端茶水或作為客廳桌面的裝飾收納都極具品味。")},
	{"Wooden Rolling Pin", ("實木烘焙桿麵棍", "打磨光滑不黏麵糰，重量適中好施力，無論包餃子或做披薩餅皮都能輕鬆擀平。")},
	{"Yellow Peeler", ("陶瓷刀片削皮刀", "鋒利且永不生鏽，貼合蔬果曲線，輕鬆削去薄皮不浪費果肉，亮色系超好找。")},

    // === 💻 筆電與 3C 周邊 (Laptops & Electronics) ===
    {"Apple MacBook Pro 14 Inch Space Grey", ("MacBook Pro 14吋 太空灰", "搭載 Apple 突破性的 M 晶片，提供難以置信的超狂效能與驚人的電池續航力。")},
	{"Asus Zenbook Pro Dual Screen Laptop", ("ASUS 雙螢幕創作者筆電", "創新的雙螢幕設計帶來前所未有的多工處理體驗，是影像創作者的最佳工作站。")},
	{"Huawei Matebook X Pro", ("華為 Matebook 觸控筆電", "極致輕薄金屬機身搭配 3K 觸控全面屏，提供令人驚豔的視覺享受與頂級商務體驗。")},
	{"Lenovo Yoga 920", ("聯想 Yoga 翻轉筆電", "獨家錶鍊式轉軸設計，筆電平板一秒切換，支援觸控筆，隨時記錄無限靈感。")},
	{"New DELL XPS 13 9300 Laptop", ("DELL XPS 13 極窄邊框筆電", "將 13 吋螢幕完美塞入 11 吋機身，採用頂級碳纖維材質，輕巧與效能的極致展現。")},
	{"Amazon Echo Plus", ("Amazon Echo Plus 智慧音箱", "內建強大 Alexa 語音助理，提供 360 度環繞音效，輕鬆聲控您的專屬智慧家庭。")},
	{"Apple Airpods", ("Apple AirPods 藍牙耳機", "開蓋即連線的魔法體驗！提供清晰音質與絕佳通話品質，感受真正的無線自由。")},
	{"Apple AirPods Max Silver", ("AirPods Max 銀色耳罩耳機", "結合高保真音質與頂尖主動降噪技術，極致舒適的耳罩設計，帶您沉浸音樂世界。")},
	{"Apple Airpower Wireless Charger", ("多設備無線充電板", "隨放隨充！支援 iPhone、Apple Watch 與 AirPods 同時充電，告別雜亂的充電線。")},
	{"Apple HomePod Mini Cosmic Grey", ("HomePod Mini 太空灰", "體積極致小巧卻能爆發驚人音量，無縫串聯 Apple 生態系，打造全屋智慧音響。")},
	{"Apple iPhone Charger", ("原廠 20W USB-C 快充頭", "提供穩定安全的快速充電體驗，只需 30 分鐘即可充入 50% 電量，效率滿分。")},
	{"Apple MagSafe Battery Pack", ("MagSafe 磁吸行動電源", "一貼即充的絕佳便利性！完美對位您的 iPhone，出門在外隨時補充戰鬥力。")},
	{"Apple Watch Series 4 Gold", ("Apple Watch S4 金色", "不僅是手錶，更是您的健康守護者。支援心電圖與跌倒偵測，隨時掌握身體狀況。")},
	{"Beats Flex Wireless Earphones", ("Beats Flex 頸掛式藍牙耳機", "磁吸式耳塞設計，提供 12 小時超長續航，是日常通勤與運動休閒的最佳伴侶。")},
	{"iPhone 12 Silicone Case with MagSafe Plum", ("iPhone 12 MagSafe 矽膠保護殼", "如絲綢般柔滑的矽膠觸感，內建磁石完美支援 MagSafe 配件，提供全方位防護。")},
	{"Monopod", ("專業級相機單腳架", "輕量化鋁合金材質，快速伸縮鎖定，為您的動態攝影與微距拍攝提供極佳穩定度。")},
	{"Selfie Lamp with iPhone", ("手機專用美顏補光燈", "多段色溫與亮度調節，一秒夾上手機，讓您在夜間或室內也能拍出無瑕網美照。")},
	{"Selfie Stick Monopod", ("藍牙遙控三腳架自拍桿", "結合自拍桿與穩固三腳架，配備分離式藍牙遙控器，出遊合照不求人。")},
	{"TV Studio Camera Pedestal", ("廣播級攝影機氣壓腳架", "專業電視台級設備，提供如絲般順滑的平移與升降運鏡，適合高階影音製作團隊。")},

    // === 👕 衣服、鞋包與配件 (Fashion, Bags & Accessories) ===
    {"Blue & Black Check Shirt", ("藍黑格紋純棉襯衫", "經典不敗的英倫格紋設計，親膚純棉材質，無論單穿或當作薄外套都極具型格。")},
	{"Gigabyte Aorus Men Tshirt", ("Aorus 電競純棉短T", "採用透氣舒適純棉材質，印有經典電競信仰 Logo，展現硬核玩家的專屬休閒穿搭。")},
	{"Man Plaid Shirt", ("經典休閒格紋襯衫", "俐落剪裁修飾身型，百搭的復古格紋圖騰，是男士衣櫃裡不可或缺的質感單品。")},
	{"Man Short Sleeve Shirt", ("修身剪裁短袖襯衫", "輕薄透氣材質，適合炎熱夏季穿著，展現乾淨俐落的微正式感，上班休閒兩相宜。")},
	{"Men Check Shirt", ("商務休閒格紋襯衫", "硬挺領型與舒適布料的完美結合，搭配西裝褲或牛仔褲，輕鬆穿出都會雅痞風。")},
	{"Nike Air Jordan 1 Red And Black", ("Air Jordan 1 經典黑紅", "風靡全球的傳奇籃球鞋！經典的黑紅配色與高筒設計，街頭潮流穿搭的終極指標。")},
	{"Nike Baseball Cleats", ("Nike 專業棒球釘鞋", "提供卓越的抓地力與爆發力，輕量化鞋身設計，助您在紅土球場上跑出最佳表現。")},
	{"Puma Future Rider Trainers", ("Puma 復古休閒老爹鞋", "結合復古外型與現代避震科技，色彩鮮明，為您的日常穿搭注入滿滿街頭活力。")},
	{"Sports Sneakers Off White & Red", ("Off-White 風格運動潮鞋", "大膽的解構設計與標誌性紅色束帶，極致吸睛的街頭話題鞋款，展現強烈個人態度。")},
	{"Sports Sneakers Off White Red", ("休閒氣墊運動鞋 白/紅", "輕量透氣網布鞋面搭配緩震氣墊，不僅外型時尚，更提供全天候行走的舒適感。")},
	{"Blue Women's Handbag", ("知性藍真皮托特包", "擁有超大容量與質感真皮手感，可放入筆電與A4文件，都會職場女性的必備通勤包。")},
	{"Heshe Women's Leather Bag", ("Heshe 歐美風真皮水桶包", "精緻工藝縫線與立體包型，隨性又不失優雅，是日常逛街與約會的最佳點綴。")},
	{"Prada Women Bag", ("Prada 經典倒三角標誌包", "象徵奢華品味的頂級精品，細緻的十字紋牛皮展現不凡身分，讓您成為全場焦點。")},
	{"White Faux Leather Backpack", ("純白極簡防潑水後背包", "輕巧無負擔的百搭包款，具備多層收納空間與防潑水機能，適合輕旅行與學生族群。")},
	{"Women Handbag Black", ("經典百搭純黑晚宴包", "雋永不敗的極簡純黑設計，低調中散發高貴氣質，完美襯托您的各式派對洋裝。")},
	{"Black Women's Gown", ("赫本風純黑無袖晚禮服", "極致優雅的立體剪裁，完美展現女性曼妙曲線，是出席重大晚宴絕對不會出錯的戰袍。")},
	{"Corset Leather With Skirt", ("性感皮革馬甲綁帶套裝", "大膽前衛的皮革材質搭配修身短裙，展現強烈個性與迷人魅力，跑趴必備吸睛穿搭。")},
	{"Corset With Black Skirt", ("蕾絲馬甲搭高腰黑裙", "甜美與性感的完美平衡，精緻蕾絲細節修飾腰線，讓您散發難以抗拒的女人味。")},
	{"Dress Pea", ("法式復古波卡圓點洋裝", "俏皮經典的圓點圖騰，搭配輕飄飄的雪紡材質，穿上瞬間自帶浪漫的法式少女感。")},
	{"Marni Red & Black Suit", ("Marni 紅黑撞色設計套裝", "高級時裝品牌的剪裁工藝，強烈的撞色對比展現自信氣場，都會女強人的時髦首選。")},
	{"Green Crystal Earring", ("翡翠綠水晶水滴耳環", "璀璨奪目的切工折射出耀眼光芒，隨著步伐搖曳生姿，為您的整體造型畫龍點睛。")},
	{"Green Oval Earring", ("復古祖母綠橢圓耳釘", "經典氣質款！低調奢華的墨綠寶石鑲嵌於精緻金屬托座，展現皇室般的古典優雅。")},
	{"Tropical Earring", ("熱帶風情流蘇造型耳環", "色彩繽紛的南洋度假風格設計，視覺效果強烈，戴上它立刻擁有飛往海島的好心情。")},
	{"Black & Brown Slipper", ("經典真皮雙帶涼拖鞋", "人體工學軟木鞋床設計，越穿越貼合腳型，百搭黑棕配色，夏日休閒穿搭必備。")},
	{"Calvin Klein Heel Shoes", ("CK 尖頭氣質高跟鞋", "完美修飾腿部線條的性感尖頭設計，舒適穩定的鞋跟，讓您每一步都散發自信光芒。")},
	{"Golden Shoes Woman", ("璀璨亮片晚宴細跟鞋", "全鞋身覆蓋閃耀亮片，宛如灰姑娘的玻璃鞋，讓您在聚光燈下成為最耀眼的女王。")},
	{"Pampi Shoes", ("百搭舒適莫卡辛豆豆鞋", "超柔軟真皮鞋面與極致彎折彈性，久走不累腳，是日常休閒與輕旅行的最佳鞋款。")},
	{"Red Shoes", ("法式優雅正紅瑪莉珍鞋", "復古可愛的繫帶設計搭配吸睛正紅色，瞬間點亮整體穿搭，散發甜美復古風情。")},

    // === ⌚ 手錶精品 (Watches) ===
    {"Brown Leather Belt Watch", ("復古真皮錶帶男仕腕錶", "搭載精準石英機芯，搭配質感頂級牛皮錶帶，完美展現成熟穩重的男士獨特品味。")},
	{"Longines Master Collection", ("浪琴巨擘系列機械錶", "傳承百年瑞士製錶工藝，優雅麥穗紋錶盤搭配藍鋼指針，象徵奢華與高雅的卓越之作。")},
	{"Rolex Cellini Date Black Dial", ("勞力士徹利尼黑面日期錶", "摒棄粗獷，展現古典之美。低調奢華的黑面盤結合實用日期功能，彰顯非凡品味。")},
	{"Rolex Cellini Moonphase", ("勞力士徹利尼月相錶", "結合隕石月相盤的極致複雜工藝，將浩瀚宇宙濃縮於腕間，是頂級鐘錶收藏家的摯愛。")},
	{"Rolex Datejust", ("勞力士日誌型經典腕錶", "全球最經典的防水腕錶之一！配備標誌性放大鏡日期窗與五珠帶，耐用且極具保值性。")},
	{"Rolex Submariner Watch", ("勞力士黑水鬼潛水錶", "傳奇的不敗神話！具備超強防水與夜光功能，無可挑惕的耐用度，霸氣十足的夢幻逸品。")},
	{"IWC Ingenieur Automatic Steel", ("IWC 萬國工程師系列自動錶", "由大師 Gérald Genta 設計的硬朗防磁錶款，精鋼錶殼展現陽剛的工程學幾何美感。")},
	{"Rolex Datejust Women", ("勞力士女裝日誌型鑽錶", "專為女性打造的優雅尺寸，鑲嵌璀璨美鑽，完美結合頂級製錶技術與高級珠寶工藝。")},
	{"Watch Gold for Women", ("米蘭金屬編織帶女錶", "閃耀細緻的金色米蘭錶帶服貼手腕，猶如精美手鍊般點綴，為您的穿搭增添輕奢質感。")},
	{"Women's Wrist Watch", ("簡約知性細帶小圓錶", "清晰易讀的極簡錶盤設計，搭配溫柔的細緻皮帶，是日常上班與約會的百搭氣質單品。")},

    // === 📱 智慧型手機與平板 (Smartphones & Tablets) ===
    {"iPhone 5s", ("Apple iPhone 5s", "改變世界的經典神機！首創 Touch ID 指紋辨識，鋁合金鑽石切邊設計，令人懷念的完美手感。")},
	{"iPhone 6", ("Apple iPhone 6", "引領大螢幕潮流的革命性機種，圓潤機身與 Retina 顯示器，流暢的系統體驗依然經典。")},
	{"iPhone 13 Pro", ("Apple iPhone 13 Pro", "支援 120Hz ProMotion 螢幕，微距攝影與電影級模式，帶您體驗無與倫比的流暢與專業拍攝。")},
	{"iPhone X", ("Apple iPhone X", "十週年紀念之作！跨時代全螢幕設計與 Face ID 臉部辨識技術，重新定義智慧型手機的未來。")},
	{"Samsung Universe 9", ("三星 Universe 9 旗艦機", "極致鮮豔的曲面 AMOLED 螢幕，搭配超大電量與強大效能，是您追劇與遊戲的絕佳神隊友。")},
	{"OPPOF19", ("OPPO F19 輕薄閃充手機", "極致輕薄機身，搭載獨家 VOOC 閃充技術，充電 5 分鐘通話 2 小時，隨時保持滿電活力。")},
	{"Huawei P30", ("華為 P30 徠卡旗艦機", "與徠卡聯合設計的專業鏡頭，超強感光夜拍能力，讓您在低光源下依然能拍出大師級美照。")},
	{"Oppo A57", ("OPPO A57 大電量娛樂機", "超大容量電池搭配立體聲雙喇叭，無論看影片或聽音樂都能擁有沈浸式的影音享受。")},
	{"Oppo F19 Pro Plus", ("OPPO F19 Pro+ 5G", "支援 5G 高速上網，配備 AI 錄影增強技術，讓您隨手拍出清晰動人的 Vlog 影像紀錄。")},
	{"Oppo K1", ("OPPO K1 螢幕指紋機", "將高階光感螢幕指紋解鎖技術帶入平價市場，水滴全螢幕帶來更廣闊的視覺衝擊。")},
	{"Realme C35", ("realme C35 極光設計機", "最美入門機！直角邊框搭配超薄機身，5000萬畫素主鏡頭讓您輕鬆記錄生活精彩瞬間。")},
	{"Realme X", ("realme X 升降鏡頭全螢幕", "炫酷升降式前鏡頭實現真正的「無瀏海」全螢幕體驗，驍龍處理器提供穩定順暢效能。")},
	{"Realme XT", ("realme XT 6400萬鷹眼機", "首款搭載 6400 萬超高畫素鏡頭，極致解析力讓您放大再放大，細節依然清晰可見。")},
	{"Samsung Galaxy S7", ("三星 Galaxy S7 防水平板", "具備 IP68 最高等級防塵防水，雙曲面玻璃設計與超快對焦鏡頭，經典旗艦的實力之作。")},
	{"Samsung Galaxy S8", ("三星 Galaxy S8 無邊際螢幕", "打破邊框限制的 Infinity Display 無邊際螢幕，猶如將整個世界握在手中般震撼。")},
	{"Samsung Galaxy S10", ("三星 Galaxy S10 超視覺旗艦", "螢幕下超聲波指紋辨識與強大的三鏡頭系統，支援無線超充分享，科技與美學的巔峰。")},
	{"Vivo S1", ("vivo S1 前置 3200 萬美顏機", "專為愛自拍的您設計！超高畫素前鏡頭搭配 AI 智慧美顏，不用修圖也能拍出仙女肌。")},
	{"Vivo V9", ("vivo V9 雙鏡頭 AI 旗艦", "極窄邊框全面屏設計，後置雙鏡頭支援硬體級人像景深，讓您輕鬆拍出單眼般的人像照。")},
	{"Vivo X21", ("vivo X21 隱形指紋手機", "引領業界的螢幕下指紋解鎖技術，極具未來感的極簡外型，給您最酷炫的解鎖體驗。")},
	{"iPad Mini 2021 Starlight", ("iPad Mini 6 星光色", "一手掌握的超級效能！全新全面屏設計搭配支援磁吸 Apple Pencil，隨身最強數位筆記本。")},
	{"Samsung Galaxy Tab S8 Plus Grey", ("三星 Galaxy Tab S8+ 平板", "超大 12.4 吋 AMOLED 螢幕，附贈超低延遲 S Pen，無論繪圖創作或觀影娛樂都無懈可擊。")},
	{"Samsung Galaxy Tab White", ("三星 Galaxy Tab 輕巧平板", "極致輕薄好攜帶，提供持久續航力與護眼模式，是孩童學習與家庭日常使用的最佳選擇。")},

    // === 🏀 運動器材 (Sports Equipment) ===
    {"American Football", ("美式足球標準訓練球", "耐磨複合皮革材質，防滑顆粒設計提供絕佳握感，適合傳球與接球等激烈美式足球賽事。")},
	{"Baseball Ball", ("標準硬式棒球", "真皮外皮搭配紅色精密雙縫線，手感紮實，提供投球時完美的轉速與握感。")},
	{"Baseball Glove", ("頂級牛皮棒球手套", "嚴選天然牛皮手工穿線，吸震護墊設計有效減輕接球衝擊，內野手與外野手防守利器。")},
	{"Basketball", ("室內外通用標準籃球", "加深溝槽設計提升控球穩定度，吸濕耐磨PU表皮，無論木地板或柏油路都能發揮最佳表現。")},
	{"Basketball Rim", ("高強度鋼製籃球框", "實心鋼材打造配備緩震彈簧，耐得住強力灌籃的衝擊，為您打造專業級的主場體驗。")},
	{"Cricket Ball", ("專業板球硬球", "採用傳統多層軟木與優質皮革縫製，硬度十足且縫線凸出，為投手提供極致的變化球路。")},
	{"Cricket Bat", ("頂級英國柳木板球蝙蝠", "精心挑選輕量且彈性極佳的柳木，極致的擊球甜蜜點，幫助擊球員輕鬆將球擊出邊界。")},
	{"Cricket Helmet", ("抗衝擊板球防護頭盔", "高強度外殼搭配堅固的鋼製護網，內部配備吸震海綿，全面保護擊球員頭部安全。")},
	{"Cricket Wicket", ("實木板球三柱門套組", "堅固耐用的實木 stump 與 bail，防守方的靈魂核心，板球賽事不可或缺的標準配備。")},
	{"Feather Shuttlecock", ("比賽級鵝毛羽毛球", "精選優質鵝毛與複合軟木球頭，飛行軌跡極致穩定不飄移，殺球速度感十足。")},
	{"Football", ("FIFA 認證標準足球", "無縫熱貼合技術確保球體完美圓潤不吸水，提供極佳的腳感與精準的飛行軌跡。")},
	{"Golf Ball", ("遠距低風阻高爾夫球", "專利空氣動力學雙渦流凹痕設計，有效降低風阻並提升極致飛行距離，果嶺上的致勝關鍵。")},
	{"Iron Golf", ("高容錯率高爾夫鐵桿", "低重心設計結合超大擊球甜蜜點，讓初學者也能輕鬆擊出高飛與筆直的好球。")},
	{"Metal Baseball Bat", ("高彈力鋁合金棒球棒", "輕量化鋁合金材質，揮擊速度更快，清脆的擊球音效與超強的反彈力，轟出全壘打的秘密武器。")},
	{"Tennis Ball", ("高彈性耐磨網球", "採用高密度耐磨織布與增壓內膽，提供持久穩定的彈跳力，適合各種硬地與紅土球場。")},
	{"Tennis Racket", ("碳纖維避震網球拍", "航太級碳纖維打造，超輕量且具備極佳減震效果，大幅降低手臂負擔，揮拍更具爆發力。")},
	{"Volleyball", ("超軟皮室內排球", "超柔軟微纖維表皮，有效減輕接球時的手臂痛感，飛行穩定，是排球校隊與聯賽首選。")},

    // === 🕶️ 太陽眼鏡 (Sunglasses) ===
    {"Black Sun Glasses", ("極黑經典偏光太陽眼鏡", "100% 阻擋紫外線並消除眩光，無論開車或戶外活動都能擁有最清晰舒適的視覺。")},
	{"Classic Sun Glasses", ("復古金屬方框墨鏡", "好萊塢明星最愛的經典不敗款，修飾臉型效果極佳，瞬間提升整體穿搭的時尚氣場。")},
	{"Green and Black Glasses", ("黑綠雙色漸層飛行員墨鏡", "大膽吸睛的漸層配色，展現強烈街頭潮流感，為您的夏日穿搭注入滿滿酷炫活力。")},
	{"Party Glasses", ("派對造型發光眼鏡", "內建 LED 炫彩燈光與趣味造型，一戴上立刻成為派對與音樂節最引人注目的焦點。")},
	{"Sunglasses", ("抗 UV400 時尚太陽眼鏡", "輕量化鏡架無壓迫感，全面防護雙眼免受紫外線傷害，夏日海島度假的必備單品。")},

    // === 🚗 汽車 (Cars) ===
    {"300 Touring", ("Chrysler 300 Touring 房車", "美式豪華房車代表作。霸氣的車頭水箱護罩與寬敞奢華的內裝，盡顯總裁級的不凡氣度。")},
	{"Charger SXT RWD", ("Dodge Charger SXT 後驅版", "純正美式肌肉車血統！強悍的馬力輸出與極具侵略性的車身線條，點燃您的熱血駕駛魂。")},
	{"Dodge Hornet GT Plus", ("Dodge Hornet GT 鋼砲休旅", "兼具休旅車的實用空間與小鋼砲的敏捷操控，帥氣外型讓您在城市穿梭中超級吸睛。")},
	{"Durango SXT RWD", ("Dodge Durango 七人座休旅", "結合肌肉車的性能與七人座的超大靈活空間，滿足全家出遊與拖曳重物的全方位需求。")},
	{"Pacifica Touring", ("Chrysler Pacifica 頂級保母車", "極致尊榮的座艙享受，配備電動滑門與豐富影音娛樂系統，為家人打造最舒適的移動城堡。")},
    
    // === 🛵 機車 (Motorcycles) ===
    {"Generic Motorcycle", ("經典復古國民檔車", "低座高且易於保養，省油耐操的經典引擎，無論日常通勤或假日輕檔車小跑都非常適合。")},
	{"Kawasaki Z800", ("Kawasaki Z800 運動街車", "極具侵略性的「淒」肌肉外型，搭載四缸引擎爆發綿密強悍動力，重機迷的夢想坐騎。")},
	{"MotoGP CI.H1", ("MotoGP 廠車級仿賽重機", "完整移植賽道頂尖科技，極致輕量化車架與猛獸級馬力，帶您體驗貼地飛行的極速快感。")},
	{"Scooter Motorcycle", ("都會通勤白牌速克達", "超大車廂置物空間與靈活的車身，起步輕快且極度省油，應付台灣擁擠市區交通的最佳利器。")},
	{"Sportbike Motorcycle", ("流線擾流全尺寸仿賽", "配備空氣動力學整流罩與低趴戰鬥坐姿，提供極佳的高速穩定性，滿足對速度的絕對渴望。")},
    
    // === 🧼 身體沐浴 (Body Wash & Lotion) ===
    {"Attitude Super Leaves Hand Soap", ("Attitude 超級葉子天然洗手乳", "萃取天然旱金蓮葉精華，溫和清潔雙手同時帶來深層保濕，散發清新淡雅的植萃香氣。")},
	{"Olay Ultra Moisture Shea Butter Body Wash", ("Olay 乳木果油極致滋潤沐浴乳", "注入高濃度乳木果油精華，洗後肌膚宛如塗過乳液般柔嫩絲滑，徹底告別乾燥脫屑。")},
	{"Vaseline Men Body and Face Lotion", ("凡士林男士臉部與身體潤膚乳", "專為男士肌膚設計的清爽無油配方，15秒快速吸收不黏膩，全天候防護乾燥與粗糙。")}
		};

		private const string FALLBACK_DESCRIPTION_TEMPLATE = "我們嚴選的 {0}，為您帶來獨特的生活體驗。原廠特色介紹：{1}";

		private class DummyJsonResponse
		{
			[JsonPropertyName("products")]
			public List<DummyProduct> Products { get; set; }
		}

		private class DummyProduct
		{
			[JsonPropertyName("title")] public string Title { get; set; }
			[JsonPropertyName("description")] public string Description { get; set; }
			[JsonPropertyName("price")] public decimal Price { get; set; }
			[JsonPropertyName("stock")] public int Stock { get; set; }
			[JsonPropertyName("category")] public string Category { get; set; }
			[JsonPropertyName("brand")] public string Brand { get; set; }
			[JsonPropertyName("images")] public List<string> Images { get; set; } = new();
		}

		public static async Task SeedAsync(ISpanShopDBContext context) // 記得確認你的 DbContext 名稱
		{
			if (context.Products.Any()) return;

			try
			{
				var dummyProducts = await FetchProductsFromApiAsync();
				if (dummyProducts == null || dummyProducts.Count == 0) return;

				var store = EnsureStoreExists(context);

				// 第三步：動態建置「多層次分類」與「品牌」
				var categories = ExtractAndCreateHierarchyCategories(context, dummyProducts);
				var brands = ExtractAndCreateBrands(context, dummyProducts);
				context.SaveChanges();

				var products = new List<Product>();
				foreach (var dummy in dummyProducts)
				{
					// 取得子分類
					var childCatName = CategoryHierarchyMap.ContainsKey(dummy.Category)
						? CategoryHierarchyMap[dummy.Category].ChildName
						: char.ToUpper(dummy.Category[0]) + dummy.Category.Substring(1);
					var category = categories.FirstOrDefault(c => c.Name == childCatName);

					// 取得翻譯後的品牌
					var rawBrand = dummy.Brand ?? "原廠直營";
					var translatedBrandName = BrandTranslationMap.ContainsKey(rawBrand)
						? BrandTranslationMap[rawBrand]
						: rawBrand;
					var brand = brands.FirstOrDefault(b => b.Name == translatedBrandName);

					int priceInTwd = (int)(dummy.Price * USD_TO_TWD);

					string productName;
					string productDescription;
					if (ProductTranslationMap.ContainsKey(dummy.Title))
					{
						var translation = ProductTranslationMap[dummy.Title];
						productName = translation.Title;
						productDescription = translation.Description;
					}
					else
					{
						productName = dummy.Title;
						productDescription = string.Format(FALLBACK_DESCRIPTION_TEMPLATE, dummy.Title, dummy.Description);
					}

					var product = new Product
					{
						StoreId = store.Id,
						CategoryId = category?.Id ?? categories.First().Id,
						BrandId = brand?.Id ?? brands.First().Id,
						Name = productName,
						Description = productDescription,
						MinPrice = priceInTwd,
						MaxPrice = priceInTwd,
						Status = 1,
						CreatedAt = DateTime.Now,
						UpdatedAt = DateTime.Now,
						ProductImages = new List<ProductImage>(),
						ProductVariants = new List<ProductVariant>()
					};

					// 處理圖片
					if (dummy.Images != null && dummy.Images.Count > 0)
					{
						for (int i = 0; i < dummy.Images.Count; i++)
						{
							product.ProductImages.Add(new ProductImage
							{
								ImageUrl = dummy.Images[i],
								IsMain = (i == 0),
								SortOrder = i
							});
						}
					}

					// 處理預設規格
					product.ProductVariants.Add(new ProductVariant
					{
						SkuCode = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
						VariantName = "標準版",
						SpecValueJson = "{}",
						Price = priceInTwd,
						Stock = _random.Next(50, 201),
						SafetyStock = 10,
						IsDeleted = false
					});

					products.Add(product);
				}

				context.Products.AddRange(products);
				await context.SaveChangesAsync();
				Console.WriteLine($"✅ 成功匯入 {products.Count} 筆商品到資料庫");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ 播種過程出錯：{ex.Message}");
				throw;
			}
		}

		private static async Task<List<DummyProduct>> FetchProductsFromApiAsync()
		{
			using var client = new HttpClient();
			var response = await client.GetAsync(DUMMYJSON_URL);
			response.EnsureSuccessStatusCode();
			var jsonContent = await response.Content.ReadAsStringAsync();
			var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
			var dummyResponse = System.Text.Json.JsonSerializer.Deserialize<DummyJsonResponse>(jsonContent, options);
			return dummyResponse?.Products ?? new List<DummyProduct>();
		}

		private static Store EnsureStoreExists(ISpanShopDBContext context) // 注意這裡的 DBContext 名稱要跟你的一致
		{
			var store = context.Stores.FirstOrDefault();
			if (store != null)
			{
				return store;
			}

			// 建立或取得 Role
			var role = context.Roles.FirstOrDefault();
			if (role == null)
			{
				role = new Role
				{
					RoleName = "Seller",
					Description = "賣家角色"
				};
				context.Roles.Add(role);
				context.SaveChanges();
			}

			// 建立 User
			var user = new User
			{
				RoleId = role.Id,
				Account = "dataseed_seller",
				Password = "hashed_password_placeholder",
				Email = "dataseed@example.com",
				IsConfirmed = true,
				IsBlacklisted = false,
				IsSeller = true,
				CreatedAt = DateTime.Now,
				UpdatedAt = DateTime.Now
			};
			context.Users.Add(user);
			context.SaveChanges();

			// 建立 Store
			store = new Store
			{
				UserId = user.Id,
				StoreName = "原廠直營",
				Description = "精選商品，品質保證",
				IsVerified = true,
				CreatedAt = DateTime.Now
			};
			context.Stores.Add(store);
			context.SaveChanges();

			return store;
		}

		/// <summary>
		/// ★★★ 建立多層次分類的核心邏輯 ★★★
		/// </summary>
		private static List<Category> ExtractAndCreateHierarchyCategories(ISpanShopDBContext context, List<DummyProduct> dummyProducts)
		{
			var flatCategoryList = new List<Category>();
			var apiCategories = dummyProducts.Select(p => p.Category).Distinct().ToList();

			foreach (var apiCat in apiCategories)
			{
				string parentName = apiCat;
				string childName = apiCat;

				if (CategoryHierarchyMap.ContainsKey(apiCat))
				{
					parentName = CategoryHierarchyMap[apiCat].ParentName;
					childName = CategoryHierarchyMap[apiCat].ChildName;
				}

				// 1. 處理主分類 (Parent)
				var parentCategory = context.Categories.FirstOrDefault(c => c.Name == parentName);
				if (parentCategory == null)
				{
					parentCategory = new Category
					{
						Name = parentName,
						Sort = 0,
						IsVisible = true,
						// ParentCategoryId = null // 如果你的資料表有這個欄位，主分類的 Parent 就是 null
					};
					context.Categories.Add(parentCategory);
					context.SaveChanges(); // 先存檔才能拿到主分類的 ID
				}

				if (!flatCategoryList.Any(c => c.Name == parentName))
					flatCategoryList.Add(parentCategory);

				// 2. 處理子分類 (Child)
				var childCategory = context.Categories.FirstOrDefault(c => c.Name == childName);
				if (childCategory == null)
				{
					childCategory = new Category
					{
						Name = childName,
						Sort = 0,
						IsVisible = true,

						/* ==========================================================
                           💡 重要提醒：
                           如果你們的 Category 資料表裡面有設計 ParentCategoryId，
                           請把下面這行的註解拿掉，這樣就能建立真正的資料庫關聯！
                           ========================================================== */
						// ParentCategoryId = parentCategory.Id 
					};
					context.Categories.Add(childCategory);
					context.SaveChanges();
				}

				if (!flatCategoryList.Any(c => c.Name == childName))
					flatCategoryList.Add(childCategory);
			}

			return flatCategoryList;
		}

		private static List<Brand> ExtractAndCreateBrands(ISpanShopDBContext context, List<DummyProduct> dummyProducts)
		{
			var brands = new List<Brand>();
			var rawBrandNames = dummyProducts.Select(p => p.Brand ?? "原廠直營").Distinct().ToList();

			foreach (var rawName in rawBrandNames)
			{
				// 使用翻譯字典轉換
				var translatedName = BrandTranslationMap.ContainsKey(rawName)
					? BrandTranslationMap[rawName]
					: rawName;

				var existing = context.Brands.FirstOrDefault(b => b.Name == translatedName);
				if (existing == null)
				{
					var brand = new Brand
					{
						Name = translatedName,
						Description = $"{translatedName} 官方直營品牌",
						LogoUrl = "https://via.placeholder.com/64x64",
						Sort = 0,
						IsVisible = true,
						IsDeleted = false
					};
					context.Brands.Add(brand);
					context.SaveChanges(); // 立即存檔確保不重複
					brands.Add(brand);
				}
				else
				{
					if (!brands.Any(b => b.Id == existing.Id))
						brands.Add(existing);
				}
			}
			return brands;
		}
	}
}