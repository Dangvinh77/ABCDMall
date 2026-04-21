import { useState } from 'react';
import { Link } from 'react-router-dom';

export const FaqFeature = () => {
  const [activeTab, setActiveTab] = useState<string>('shopping');
  const [activeQuestion, setActiveQuestion] = useState<number | null>(null);

  const categories = [
    { id: 'shopping', name: '🛍️ Shopping & Services' },
    { id: 'amenities', name: '🚗 Amenities & Parking' },
    { id: 'cinema', name: '🎬 Cinema' },
    { id: 'other', name: '💡 Other Policies' }
  ];

  const faqs: Record<string, { q: string; a: string }[]> = {
    shopping: [
      { q: 'What are the operating hours of ABCD Mall?', a: 'ABCD Mall is open from 09:30 to 22:00 every day of the week, including holidays and Sundays. The cinema area and some restaurants may close later.' },
      { q: 'How can I request a VAT invoice?', a: 'Please request your VAT invoice directly at the store cashier at the time of purchase. The mall does not issue combined invoices for multiple stores.' },
      { q: 'Does the mall have a membership program?', a: 'Yes. You can register for the ABCD Member program at the information desk on Floor 1 or through the mobile app to collect points and receive exclusive offers from more than 200 brands.' }
    ],
    amenities: [
      { q: 'Where is the parking area and what are the fees?', a: 'ABCD Mall offers motorcycle parking at basement levels B1 and B2, and car parking on levels 3M and 5. Motorcycle parking starts at 5,000 VND for the first 4 hours, while car parking starts at 30,000 VND for the first 4 hours.' },
      { q: 'Does the mall provide wheelchairs for seniors or guests with disabilities?', a: 'Yes. ABCD Mall offers free wheelchair rental. Please contact the information desk on Floor 1 and present an ID document for assistance.' },
      { q: 'Does the mall have a baby care room?', a: 'Yes. Baby care rooms are available on Floors 2 and 3, equipped with diaper-changing stations, hot and cold water, and private nursing areas.' }
    ],
    cinema: [
      { q: 'Can I buy movie tickets online through this website?', a: 'Yes. You can visit the Cinema section in the navigation bar to choose a movie, select a showtime, pick your seats, and pay online.' },
      { q: 'At what age do children receive free movie admission?', a: 'According to cinema policy, children under 90 cm in height can enter for free when sharing a seat with an accompanying adult.' }
    ],
    other: [
      { q: 'Are pets allowed inside the mall?', a: 'To ensure safety and hygiene for all visitors, ABCD Mall currently does not allow pets inside the mall, except for guide dogs assisting visually impaired guests.' },
      { q: 'I lost an item at the mall. Who should I contact?', a: 'If you lose personal belongings, please visit the information desk on Floor 1 or call the hotline at 1800-ABCD-MALL for assistance from the security team.' }
    ]
  };

  const toggleQuestion = (index: number) => {
    setActiveQuestion(activeQuestion === index ? null : index);
  };

  return (
    <div className="bg-slate-50 min-h-screen pt-32 pb-20">
      <div className="max-w-4xl mx-auto px-6">
        
        <div className="text-center mb-12">
          <h1 className="text-4xl md:text-5xl font-black uppercase text-transparent bg-clip-text bg-gradient-to-r from-red-600 to-orange-500 mb-4 tracking-tight">
            Frequently Asked Questions
          </h1>
          <p className="text-gray-500 text-lg">We are always ready to answer your questions and help deliver the best possible experience at ABCD Mall.</p>
        </div>

        {/* Tab Categories */}
        <div className="flex flex-wrap justify-center gap-3 mb-10">
          {categories.map(cat => (
            <button
              key={cat.id}
              onClick={() => { setActiveTab(cat.id); setActiveQuestion(null); }}
              className={`px-6 py-3 rounded-full font-bold transition-all duration-300 shadow-sm ${
                activeTab === cat.id 
                  ? 'bg-gradient-to-r from-red-500 to-orange-500 text-white scale-105' 
                  : 'bg-white text-gray-600 hover:text-red-500 border border-gray-200'
              }`}
            >
              {cat.name}
            </button>
          ))}
        </div>

        {/* FAQ Accordion */}
        <div className="bg-white rounded-[2rem] shadow-xl border border-gray-100 p-6 md:p-10">
          <div className="space-y-4">
            {faqs[activeTab].map((faq, index) => (
              <div key={index} className="border border-gray-100 rounded-2xl overflow-hidden transition-all duration-300">
                <button
                  onClick={() => toggleQuestion(index)}
                  className={`w-full text-left px-6 py-5 flex justify-between items-center transition-colors ${
                    activeQuestion === index ? 'bg-red-50' : 'bg-white hover:bg-gray-50'
                  }`}
                >
                  <span className={`font-bold text-lg pr-4 ${activeQuestion === index ? 'text-red-600' : 'text-gray-800'}`}>
                    {faq.q}
                  </span>
                  <span className={`text-2xl transition-transform duration-300 ${activeQuestion === index ? 'rotate-180 text-red-500' : 'text-gray-400'}`}>
                    ↓
                  </span>
                </button>
                
                <div 
                  className={`overflow-hidden transition-all duration-300 ease-in-out ${
                    activeQuestion === index ? 'max-h-96 opacity-100' : 'max-h-0 opacity-0'
                  }`}
                >
                  <div className="px-6 pb-5 pt-2 text-gray-600 leading-relaxed border-t border-red-100">
                    {faq.a}
                  </div>
                </div>
              </div>
            ))}
          </div>

          {/* Call to action */}
          <div className="mt-12 pt-8 border-t border-gray-200 text-center">
            <p className="text-gray-500 mb-4">Couldn&apos;t find the answer you needed?</p>
            <Link to="/feedback" className="inline-block bg-gray-900 text-white font-bold px-8 py-3 rounded-full hover:bg-red-600 transition-colors shadow-md">
              Send Us Feedback
            </Link>
          </div>
        </div>

      </div>
    </div>
  );
};
