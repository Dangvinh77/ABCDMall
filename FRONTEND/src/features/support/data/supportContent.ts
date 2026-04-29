export interface SupportContentEntry {
  id: string;
  categoryKey: string;
  categoryLabel: string;
  question: string;
  answer: string;
}

export const supportContent: SupportContentEntry[] = [
  {
    id: "booking-how-to-book",
    categoryKey: "booking",
    categoryLabel: "Booking",
    question: "How do I book movie tickets online?",
    answer:
      "Go to Showtimes, choose your movie and screening time, pick available seats, then complete payment with the email address where you want to receive your e-ticket.",
  },
  {
    id: "booking-account-required",
    categoryKey: "booking",
    categoryLabel: "Booking",
    question: "Do I need to create an account before booking a movie ticket?",
    answer:
      "No. You can book as a guest. Just make sure your email address is correct because the booking confirmation and ticket email will be sent there.",
  },
  {
    id: "tickets-resend-email",
    categoryKey: "tickets",
    categoryLabel: "Tickets & Email",
    question: "What should I do if I did not receive my movie ticket email?",
    answer:
      "Check your Spam or Junk folder first. If the ticket still does not appear, use the resend form at the top of this page with the booking email and booking code.",
  },
  {
    id: "tickets-booking-code",
    categoryKey: "tickets",
    categoryLabel: "Tickets & Email",
    question: "Where can I find my booking code?",
    answer:
      "Your booking code appears in the confirmation screen after payment and inside the ticket email sent to the address you entered during checkout.",
  },
  {
    id: "showtimes-arrival-time",
    categoryKey: "showtimes",
    categoryLabel: "Showtimes & Arrival",
    question: "How early should I arrive before my movie starts?",
    answer:
      "Please arrive at least 15 to 20 minutes before showtime so you have enough time for ticket checks, concessions, and finding your seat before the trailers end.",
  },
  {
    id: "showtimes-missed-time",
    categoryKey: "showtimes",
    categoryLabel: "Showtimes & Arrival",
    question: "Can I still enter if I arrive after the showtime has started?",
    answer:
      "Late entry depends on cinema staff instructions and hall conditions. To avoid disruption, you should arrive before the scheduled showtime whenever possible.",
  },
  {
    id: "promotions-where-to-check",
    categoryKey: "promotions",
    categoryLabel: "Promotions",
    question: "Where can I check the latest movie promotions?",
    answer:
      "Visit the Promotions page from the movies section to see the latest ticket discounts, partner offers, and limited-time cinema campaigns.",
  },
  {
    id: "promotions-how-to-use",
    categoryKey: "promotions",
    categoryLabel: "Promotions",
    question: "How do I apply a promotion when booking tickets?",
    answer:
      "Open the eligible promotion from the movies area, review its conditions, and continue to the linked booking flow. Some offers are applied automatically only for matching showtimes or payment methods.",
  },
];
