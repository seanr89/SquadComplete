import React, { useEffect, useState } from 'react';
import { fetchFixture } from '../api';

interface FixtureInfoProps {
  fixtureId: number;
}

const FixtureInfo: React.FC<FixtureInfoProps> = ({ fixtureId }) => {
  const [fixture, setFixture] = useState<any>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    let isMounted = true;
    const loadFixture = async () => {
      setLoading(true);
      const data = await fetchFixture(fixtureId);
      if (isMounted) {
        setFixture(data);
        setLoading(false);
      }
    };
    if (fixtureId) {
      loadFixture();
    }
    return () => {
      isMounted = false;
    };
  }, [fixtureId]);

  if (loading) {
    return <div className="text-xs text-slate-500 animate-pulse mt-1">Loading fixture info...</div>;
  }

  if (!fixture) {
    return null;
  }

  const { homeTeamName, awayTeamName, homeGoalCount, awayGoalCount } = fixture;
  const fixtureDate = fixture.fixtureDate || fixture.fixture_date;

  const hasScore = homeGoalCount !== null && awayGoalCount !== null;

  return (
    <div className="bg-slate-900/50 rounded-lg p-3 mt-3 border border-slate-700/50 inline-flex flex-col gap-2 w-full md:w-auto">
      <div className="flex items-center gap-3 text-sm">
        <div className="font-semibold text-slate-300 flex-1 text-right">
          {homeTeamName}
        </div>
        
        {hasScore ? (
          <div className="bg-slate-800 px-3 py-1 rounded-md font-black text-yellow-400 min-w-[60px] text-center border border-slate-700">
            {homeGoalCount} - {awayGoalCount}
          </div>
        ) : (
          <div className="bg-slate-800 px-2 py-1 rounded-md font-bold text-slate-400 text-xs">
            VS
          </div>
        )}

        <div className="font-semibold text-slate-300 flex-1">
          {awayTeamName}
        </div>
      </div>

      {fixtureDate && (
        <div className="text-[10px] text-slate-400 font-semibold tracking-wider text-center flex items-center justify-center gap-1.5 border-t border-slate-800/80 pt-2 mt-1">
          <svg className="w-3.5 h-3.5 text-slate-500" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
          </svg>
          <span>
            {new Date(fixtureDate).toLocaleDateString(undefined, {
              weekday: 'short',
              year: 'numeric',
              month: 'short',
              day: 'numeric',
            })}
          </span>
        </div>
      )}
    </div>
  );
};

export default FixtureInfo;
