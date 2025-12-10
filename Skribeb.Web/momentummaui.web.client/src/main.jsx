import React from 'react';
import ReactDOM from 'react-dom/client';
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import Layout from '../Layout.jsx'; // Correctly import the renamed Layout
import './index.css'; // Your main stylesheet

// --- Create your page components ---
// You will need to create these files. For now, they can be simple placeholders.
const JournalPage = () => <h1 className="p-8 text-2xl font-bold">Journal Page</h1>;
const HistoryPage = () => <h1 className="p-8 text-2xl font-bold">History Page</h1>;
// In a real app, you would import these from their own files:
// import JournalPage from './pages/JournalPage';
// import HistoryPage from './pages/HistoryPage';

// Create a client for React Query
const queryClient = new QueryClient();

ReactDOM.createRoot(document.getElementById('root')).render(
    <React.StrictMode>
        <QueryClientProvider client={queryClient}>
            <BrowserRouter>
                <Routes>
                    {/* This sets up the Layout as the parent for your pages */}
                    <Route element={<Layout />}>
                        {/* The default page to show will be the Journal page */}
                        <Route path="/" element={<JournalPage />} />
                        <Route path="/journal" element={<JournalPage />} />
                        <Route path="/history" element={<HistoryPage />} />
                    </Route>
                </Routes>
            </BrowserRouter>
        </QueryClientProvider>
    </React.StrictMode>
);