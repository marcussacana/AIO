﻿using System.Collections.Generic;
using System.Text;
using System;

namespace AutomataTranslator {

    public enum Language {
      Null = -1,  JP = 0, EN = 1, FR = 2, IT = 3, GE = 4 , SP = 5, KO = 6, CH = 7
    }
    public class BinTL : PluginBase {
        MRubyStringEditor Editor;
        Language TargetLang;
        string[] Strs;
        public Dictionary<int, int> IndexMap { private set; get; } = new Dictionary<int, int>();
        public Dictionary<int, Language> LanguageMap { private set; get; } = new Dictionary<int, Language>();

        public List<Language> ReplacesTargets = new List<Language>();

        /*
        /// <summary>
        /// Try Detect the string language by the string index
        /// </summary>
        public bool IndexCheck = true;
        */
        /// <summary>
        /// Remove languages in the black list to generate a script more stable (prevent corrupt bytecode offests)
        /// </summary>
        public bool RemoveLangs = true;

        /// <summary>
        /// Import include only the target language.
        /// </summary>
        public bool ImportTargetLangOnly = true;

        /// <summary>
        /// If the result script size are too big, automatically append the remove lang list and try again.
        /// </summary>
        public bool AutoRemoveLang = true;

        public BinTL(byte[] Script, Language Lang = Language.EN) {
            TargetLang = Lang;
            Editor = new MRubyStringEditor(Script);
        }
        public BinTL(byte[] Script) {
            TargetLang = Language.EN;
            Editor = new MRubyStringEditor(Script);
        }

        public override string[] Import() {
            Strs = Editor.Import();
            List<string> Strings = new List<string>();
            int lastjap = 0;
            for (int i = 0; i < Strs.Length; i++) {/*
                if (Strs.Length % 8 == 0 && IndexCheck) {
                    LanguageMap[i] = (Language)(i % 8);
                } else {*/
                bool jap = IsJap(Strs[i]);
                if (jap)
                    lastjap = i;
                else if (i > 0 && IsEng(Strs[i], Strs[i-1]) && i - lastjap > 7) {
                    lastjap = i - 1;
                    LanguageMap[i - 1] = Language.JP;
                } else if (IsKor(Strs[i])) {
                    lastjap = i - (int)Language.KO;
                }
                int Diff = i - lastjap;
                Language Lang = (Language)(Diff % 8);
                LanguageMap[i] = Lang;

                if (LanguageMap[i] == TargetLang || !ImportTargetLangOnly) {
                    IndexMap[Strings.Count] = i;
                    Strings.Add(Strs[i]);
                }
            }
            return Strings.ToArray();
        }

        private bool IsEng(string str, string last) {
            int c = 0;
            foreach (char ch in str)
                if (ch >= 48 && ch <= 126)
                    c++;
            if (last == null)
                return (c > str.Length / 4);
            else
                return (c > str.Length / 4 && !IsEng(last, null));
        }

        private bool IsKor(string str) {
            int c = 0;
            foreach (char ch in str)
                if (Kor.Contains(ch.ToString()))
                    c++;
            return (c > str.Length / 4);
        }

        private const string Jap = "ぁあぃいぅうぇえぉおかがきぎくぐけげこごさざしじすずせぜそぞただちぢっつづてでとどなにぬねのはばぱひびぴふぶぷへべぺほぼぽまみむめもゃやゅゆょよらりるれろゎわゐゑをんゔゕゖゞゟァアィイゥウェエォオカガキギクグケゲコゴサザシジスズセゼソゾタダチヂッツヅテデトドナニヌネノハバパヒビピフブプヘベペホボポマミムメモャヤュユョヨラリルレロヮワヰヱヲンヴヵヶヷヸヹヺ･ｦｧｨｩｪｫｬｭｮｯｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝ？、。.一";
        private const string Kor = "ㄱㄴㄷㄹㅁㅂㅅㅇㅈㅊㅋㅌㅍㅎㄲㄸㅃㅆㅉㅏㅐㅑㅓㅔㅕㅗㅘㅛㅜㅠㅡㅢㅣ가각갂간갇갈갉감갑값갓갔강갖갗같갚갛개객갠갤갬갭갯갰갱갸갹걀걔거걱건걷걸걹검겁것겄겅겆겉겊겋게겐겔겜겟겠겡겨격겪견겯결겸겹겼경곁계곗고곡곤곧골곪곬곯곰곱곳공곶곺과곽관괄괌괍괏광괘괙괜괠괨괩괭괴괸괼굄굉교구국군굳굴굵굶굼굽굿궁궂궈권궐궤귀귄귈귐귑귓규균귤귬그극근귿글긁금급긋긍기긱긴긷길김깁깃깄깅깊까깍깎깐깔깜깝깠깡깢깥깨깩깬깰깸깹깻깼깽꺄꺅꺼꺽꺾껀껄껌껍껏껐껑께껜껴꼍꼬꼭꼰꼴꼼꼽꼿꽁꽂꽃꽈꽉꽌꽘꽜꽝꽤꽥꽹꾀꾄꾈꾐꾕꾜꾸꾹꾼꿀꿇꿈꿉꿋꿍꿎꿔꿘꿩꿰뀀뀌뀍뀐뀔뀜뀝뀨끄끅끈끊끌끓끔끕끗끙끝끼끽낀낄낌낍낏낐낑나낙낚난낟날낡남납낫났낭낮낯낱낳내낵낸낻낼냄냅냇냈냉냐냑냠냥냬너넉넋넌넏널넓넘넙넛넜넝넞넣네넥넨넬넴넵넷녀녁년념녑녓녔녕녘녜노녹녺논놀놈놉놋농높놓놔놨뇌뇐뇔뇜뇨뇽누눅눈눋눌눓눔눕눗눠눳눴뉘뉜뉠뉨뉩뉴늄늉느늑는늘늙늠늡능늦늪늬니닉닌닐님닙닛닝닢다닥닦단닫달닭닮닳담답닷당닺닻닿대댁댄댈댐댑댓댔댕더덕던덛덜덟덤덥덧덨덩덪덫덮데덱덴델뎀뎃뎅뎌도독돈돋돌돐돔돕돗동돛돼됀됐되된될됨됩됫됬두둑둔둘둠둡둣둥둬뒀뒈뒤뒷듀듐드득든듣들듬듭듯등디딕딘딛딜딤딥딧딨딩딪따딱딲딴딷딸땀땁땃땄땅땋때땍땐땔땜땝땟땠땡떠떡떤떨떫떰떱떳떴떵떻떼뗀뗄뗌뗏뗑또똑똔똘똥뙤뚜뚝뚤뚫뚬뚱뛰뛴뛸뜀뜁뜨뜩뜬뜯뜰뜸뜹뜻띄띈띠띤띨띰띱띵라락란랄람랍랏랐랑랗래랙랜랠램랩랫랬랭랴략량러럭런럴럼럽럿렀렁렇레렉렌렐렘렙렛렝려력련렬렴렵렷렸령례로록론롤롬롭롯롱뢰료룡루룩룬룰룸룹룻룽뤄뤼류륙륜률륨륭르륵른를름릅릇릉릎리릭린릴림립릿링마막만많맏말맑맘맙맛망맞맡맣매맥맨맬맴맵맷맸맹맺먀머먹먼멀멈멉멋멍멎멓메멕멘멜멤멥멧며멱면멸몃몄명몇모목몫몬몰몸몹못몽뫃뫼묏묘무묵묶문묻물묽뭄뭇뭉뭍뭏뭐뭔뭘뭡뭣뮈뮌뮤므믄믈믐미믹민믿밀밈밉밋밍및밑뮴바박밖반받발밝밟밤밥밧방밭배백밴밷밸뱀뱁뱃뱄뱅버벅번벋벌범법벗벘벙벚베벡벤벧벨벰벱벳벵벼벽변별볌볍볏볐병볒볕보복볶본볼봄봅봇봉봐봔봤뵈뵉뵌뵐뵘뵙뵤부북분붇불붉붐붑붓붕붙붜뷔뷰브븍븐블븜븝븟비빅빈빋빌빎빔빕빗빙빚빛빠빡빤빨빰빱빳빴빵빻빼빽뺀뺄뺌뺏뺐뺑뺘뺨뻐뻑뻔뻗뻘뻠뻣뻤뻥뻬뼈뼉뼘뼝뽀뽁뽄뽈뽐뽑뽕뾰뿀뿅뿌뿍뿐뿔뿜뿝뿡쁘쁙쁜쁠쁨쁩삐삑삔삘삠삣삥사삭삮삯산삳살삵삶삼삽삿샀상샅새색샌샏샐샘샙샛샜생샤샥샨샬샴샷샹서석섞선섣설섥섦섧섪섬섭섯섰성섶세섹센셀셈셉셋셍셑셔션셧셨셰소속솎손솔솜솝솟송솥솨솩솰쇄쇈쇗쇠쇤쇨쇰쇱쇳쇼쇽숀숄숍수숙순숟술숨숩숫숭숯숱숲숴쉐쉘쉬쉰쉴쉼쉽쉿슁슈슉슐슘슛슝스슥슨슬슭슴습슷승시식신싣실싫심십싯싱싶싸싹싼쌀쌈쌉쌋쌌쌍쌓쌔쌕쌘쌜쌤쌧쌨쌩쌰썃썅써썩썬썰썲썸썹썼썽쎄쎈쎙쏘쏙쏜쏟쏠쏨쏭쏴쏵쐐쐬쐰쐴쐼쐿쑤쑥쑨쑬쑴쑹쒀쒔쓔쓕쓰쓱쓴쓸씀씁씌씐씨씩씬씰씸씹씻씽아악안앉않알앎앓암압앗았앙앞앟애액앤앨앰앱앳앴앵야약얀얄얇얌얍얏얐양얕얗얘얜어억얶언얹얺얻얼얽엄업없엇었엉엊엌엎에엑엔엘엠엡엣엥여역엮연열엶엷염엽엾엿였영옆옇예옌옐옛오옥온올옭옮옳옴옵옷옹옺옻와왁완왈왐왑왓왔왜왝왠왱외왹왼욀욈욋욍요욕욘욥욧용우욱운욷울욹욺움웁웃웅워웍원월웜웝웟웠웡웨웩웬웰웸위윅윈윌윔윗윙유육윤율윰융윷으윽은을읊음읍응의이익인일읽잃임입잇있잉잊잎왕자작잔잖잘잠잡잣잤장잦잧재잭잰잴잼잽잿쟀쟁쟈쟉쟌쟘저적전절젊점접젓젔정젖제젝젠젤젬젭젯져젼졌조족존졸졺좀좁종좆좇좋좌좍좐좔좜좝좟좠좨좬좰좸죄죈죌죔죕죗죠죤죰죱죵주죽준줄줆줌줍줏중줘줬쥐쥔쥘쥠쥡쥬쥰쥴즈즉즌즐즘즙증지직진짇질짊짐집짓징짖짙짚짜짝짠짢짤짧짬짭짯짰짱째짹짼쨀쨈쨉쨋쨌쨍쨘쩌쩍쩐쩔쩜쩝쩡쪄쪘쪼쪽쫀쫄쫌쫍쫑쫒쫓쫘쫙쬐쬔쬘쬠쬡쭁쭈쭉쭐쭘쭙쭝쭤쮸쯔쯤쯧찌찍찐찔찜찝찡찢찧차착찬찮찯찰참찹찻찼창찾찿채책챈챌챔챕챘챙챠챤챰챱처척천철첨첩첫청체첵첸첼쳇쳐쳤초촉촌촐촘촙촛총촬최쵸추축춘출춤춥춧충춰췄췌취츄츠측츰층치칙친칠칡침칩칫칭카칵칸칼캄캅캇캉캐캑캔캘캠캡캣캥캬캭커컨컬컴컵컷컹케켁켄켈켐켑켓켕켜켠켤켬켭켯켰코콕콘콜콤콥콧콩콰콱콴콸쾀쾅쾌쾍쾡쾰쿄쿠쿡쿤쿨쿰쿳쿵쿼퀘퀭퀴퀵퀸큐크큰클큼큽킁키킥킨킬킴킵킷킹타탁탄탈탉탐탑탓탔탕태택탠탤탬탭탯탰탱터턱턴털텀텁텃텄텅테텍텐텔템텝텟텠텡텨토톡톤톨톰톱톳통퇘퇴투툭툰툴툼툽툿퉁퉤퉷튀튄튈튐튑튓튕튜튬트특튼튿틀틈틉틋틔퇸티틱틴틸팀팁팅파팍팎판팔팜팝팟팠팡팥패팩팬팰팸팹팻팼팽퍼퍽펀펄펌펍펏펐펑페펙펜펠펨펩펫펭펴펵편펼폄폅폈평폐포폭폰폴폼폽폿퐁푀푄표푯푸푹푼풀품풉풋풍퓌퓨프픈플픔픕피픽핀필핌핍핏핑하학한할핥함합핫핬항핱해핵핸핼햄햅햇했행햐향허헉헌헐헒험헙헛헝헤헥헨헬헴헵헷헹혀혁현혈혐협혓혔형혜호혹혼홀홈홉홋홍화확환활홧황홰홱횃회획횟횡효후훅훈훌훑훔훗훠훤훨훰훼휀휑휘휙휜휠휨휩휭휴휼흄흉흐흑흔흗흘흙흠흡흣흥흩희흰흴히힉힌힐힘힙힛힝ㆁㆆㅿ괵굻궉궝궹귕긇긎긏긑긔깟껭꼉꽐꾿끠맄앍앏앒얫얱옜옝웽윗윳쭌";
        private bool IsJap(string str) {
            //65281-65434
            //12288-12589
            int count = 0;
            foreach (char c in str)
                if ((c >= 65281 && c <= 65434) || (c >= 12288 && c <= 12589) || Jap.Contains(c.ToString()))
                    count++;
            return (count > str.Length / 4);
        }

        public override byte[] Export(string[] Strings) {
            if (RemoveLangs && ReplacesTargets.Count == 0) {
                ReplacesTargets.Add(Language.KO);
            }
            string[] NewStrs = new string[Strs.Length];
            Strs.CopyTo(NewStrs, 0);
            for (int i = 0; i < Strings.Length; i++)
                NewStrs[IndexMap[i]] = Strings[i];
            int SpaceRequired = Editor.CalculateLength(NewStrs) - Editor.StringTableLength;
            if (SpaceRequired < 0 && RemoveLangs) {
                for (int i = 0; i < Strs.Length - 5; i++) {
                    if (NewStrs[i].Contains("_") || NewStrs[i].Contains("@") || NewStrs[i].Contains("#"))
                        continue;
                    if ((LanguageMap[i] != Language.JP && LanguageMap[i] != Language.CH && LanguageMap[i] != Language.KO && NewStrs[i].ToUpper() == NewStrs[i]) || TargetLang == LanguageMap[i])
                        continue;
                    else {
                        while (SpaceRequired++ < 0)
                            NewStrs[i] += ' ';
                        break;
                    }
                }

            }
            SpaceRequired = Editor.CalculateLength(NewStrs) - Editor.StringTableLength;
            for (int i = 0; i < Strs.Length - 5 && RemoveLangs && SpaceRequired > 0; i++)
                if (ReplacesTargets.Contains(LanguageMap[i]) && LanguageMap[i] != TargetLang) {
                    if (NewStrs[i].Contains("_") || NewStrs[i].Contains("@") || NewStrs[i].Contains("#") || (LanguageMap[i] != Language.JP && LanguageMap[i] != Language.CH && LanguageMap[i] != Language.KO && NewStrs[i].ToUpper() == NewStrs[i]))
                        continue;//ignore if is a system script...
                    SpaceRequired -= Encoding.UTF8.GetByteCount(NewStrs[i]) - 1;
                    NewStrs[i] = " ";
                }
            try {
                return Editor.Export(NewStrs);
            }
            catch {
                bool Ok = false;
                if (AutoRemoveLang)
                    for (Language i = Language.CH; i != Language.Null; i--) {
                        if (i == TargetLang)
                            continue;
                        if (ReplacesTargets.Contains(i))
                            continue;
                        Ok = true;
                        ReplacesTargets.Add(i);
                        break;
                    }
                if (!Ok)
                    throw new System.Exception("Strings too long...");
                return Export(Strings);
            }
        }

    }
}
