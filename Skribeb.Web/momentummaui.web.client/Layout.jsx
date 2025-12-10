import React, { useEffect, useState } from "react";
import { Link, useLocation } from "react-router-dom";
import { createPageUrl } from "@/utils";
import { BookOpen, Calendar } from "lucide-react";
import { useQuery } from "@tanstack/react-query";

export default function Layout({ children, currentPageName }) {
    const location = useLocation();

    const { data: user } = useQuery({
        queryKey: ["currentUser"],
        queryFn: () => base44.auth.me(),
    });

    const theme = user?.theme || "light";

    useEffect(() => {
        document.documentElement.className = theme;
    }, [theme]);

    const isActive = (pageName) => {
        return location.pathname === createPageUrl(pageName);
    };

    return (
        <div className="min-h-screen bg-background text-foreground transition-colors duration-300">
            <style>{`
        :root {
          --background: #FAFAF9;
          --foreground: #1C1917;
          --card: #FFFFFF;
          --card-foreground: #1C1917;
          --primary: #7C3AED;
          --primary-foreground: #FFFFFF;
          --secondary: #F5F5F4;
          --secondary-foreground: #1C1917;
          --muted: #F5F5F4;
          --muted-foreground: #78716C;
          --accent: #F5F5F4;
          --accent-foreground: #1C1917;
          --border: #E7E5E4;
        }

        .dark {
          --background: #0C0A09;
          --foreground: #FAFAF9;
          --card: #1C1917;
          --card-foreground: #FAFAF9;
          --primary: #8B5CF6;
          --primary-foreground: #FFFFFF;
          --secondary: #292524;
          --secondary-foreground: #FAFAF9;
          --muted: #292524;
          --muted-foreground: #A8A29E;
          --accent: #292524;
          --accent-foreground: #FAFAF9;
          --border: #292524;
        }

        .bg-background { background-color: var(--background); }
        .text-foreground { color: var(--foreground); }
        .bg-card { background-color: var(--card); }
        .text-card-foreground { color: var(--card-foreground); }
        .bg-primary { background-color: var(--primary); }
        .text-primary { color: var(--primary); }
        .text-primary-foreground { color: var(--primary-foreground); }
        .bg-secondary { background-color: var(--secondary); }
        .text-muted-foreground { color: var(--muted-foreground); }
        .border-border { border-color: var(--border); }
      `}</style>

            <div className="pb-20 md:pb-0">
                {children}
            </div>

            {/* Mobile Bottom Navigation */}
            <nav className="fixed bottom-0 left-0 right-0 bg-card border-t border-border md:hidden z-50">
                <div className="flex justify-around items-center h-16">
                    <Link
                        to={createPageUrl("Journal")}
                        className={`flex flex-col items-center gap-1 px-6 py-2 transition-colors ${isActive("Journal") ? "text-primary" : "text-muted-foreground"
                            }`}
                    >
                        <BookOpen className="w-6 h-6" />
                        <span className="text-xs font-medium">Journal</span>
                    </Link>
                    <Link
                        to={createPageUrl("History")}
                        className={`flex flex-col items-center gap-1 px-6 py-2 transition-colors ${isActive("History") ? "text-primary" : "text-muted-foreground"
                            }`}
                    >
                        <Calendar className="w-6 h-6" />
                        <span className="text-xs font-medium">History</span>
                    </Link>
                </div>
            </nav>

            {/* Desktop Sidebar */}
            <nav className="hidden md:flex fixed left-0 top-0 bottom-0 w-20 bg-card border-r border-border flex-col items-center py-8 gap-6 z-50">
                <Link
                    to={createPageUrl("Journal")}
                    className={`flex flex-col items-center gap-2 p-3 rounded-xl transition-all ${isActive("Journal")
                            ? "bg-primary text-primary-foreground"
                            : "text-muted-foreground hover:bg-secondary"
                        }`}
                >
                    <BookOpen className="w-6 h-6" />
                    <span className="text-xs">Journal</span>
                </Link>
                <Link
                    to={createPageUrl("History")}
                    className={`flex flex-col items-center gap-2 p-3 rounded-xl transition-all ${isActive("History")
                            ? "bg-primary text-primary-foreground"
                            : "text-muted-foreground hover:bg-secondary"
                        }`}
                >
                    <Calendar className="w-6 h-6" />
                    <span className="text-xs">History</span>
                </Link>
            </nav>
        </div>
    );
}